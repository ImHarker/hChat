﻿using hChatAPI.Models;
using hChatAPI.Models.Requests;
using hChatAPI.Services;
using hChatAPI.Services._2FA;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Swashbuckle.AspNetCore.Annotations;
using static hChatAPI.Services.JWTService;

namespace hChatAPI.Controllers {
	[ApiController]
	[Route("[controller]")]
	public class AuthController : ControllerBase {
		private readonly ILogger<AuthController> _logger;
		private readonly UserService _userService;
		private readonly JwtService _jwtService;
		private readonly DataContext _context;


		public AuthController(ILogger<AuthController> logger, UserService userService, JwtService jwtService, DataContext context) {
			_logger = logger;
			_userService = userService;
			_jwtService = jwtService;
			_context = context;
		}


		[HttpPost("register")]
		public IActionResult Register([FromBody]UserAuthRequest request) {
				
			if (!ModelState.IsValid) {
					return BadRequest(ModelState);
			}

			try {
				var user = _userService.Register(request);
				return Ok(user);
			}
			catch (UserRegistrationException e) {
				return BadRequest(e.Message);
			}

		}
		
		[HttpPost("login")]
		public async Task<IActionResult> LoginAsync([FromBody] UserAuthRequest request) {

			if (!ModelState.IsValid) {
				return BadRequest(ModelState);
			}

			try {
				var user = _userService.Login(request);
				await _userService.Revoke(user.Username);
				return Ok(_jwtService.GenerateAccessToken(user.Username));
			} catch (UserAuthenticationException e) {
				return BadRequest(e.Message);
			}

		}

		[SwaggerOperation(Tags = ["Test"])]
		[HttpGet("protected")]
		[Authorize]
		public IActionResult ProtectedEndpoint() {
			return Ok("This is a protected endpoint.");
		}

		[HttpGet("revoke")]
		[Authorize]
		public async Task<IActionResult> RevokeAsync() {
			try {
				await _userService.Revoke(User.Claims.First(c => c.Type == "userId").Value);
			}
			catch (InvalidOperationException e) {
				return BadRequest();
			}
			return Ok("Revoked tokens.");
		}

		[HttpGet("logout")]
		[Authorize]
		public async Task<IActionResult> LogoutAsync() {
			try {
				await _userService.Revoke(User.Claims.First(c => c.Type == "userId").Value);
			} catch (InvalidOperationException e) {
				return BadRequest();
			}
			return Ok("Logged Out.");
		}
		
		[SwaggerOperation(Tags = ["2FA"])]
		[HttpGet("setup2fa")]
		[Authorize]
		public async Task<IActionResult> Setup2FA() {
			var username = User.Claims.First(c => c.Type == "userId").Value;
			var user = await _context.Users.Include(user => user.User2FA).FirstOrDefaultAsync(u => u.Username == username);
			if(user == null) return BadRequest();
			if (user.User2FA.Is2FAEnabled) return BadRequest("2FA is already enabled");

			var resp = await _userService.Setup2FA(user.Username);
			var challengeToken = _jwtService.GenerateChallengeToken(user.Username);
			resp.ChallengeToken = challengeToken;
			return Ok(resp);
		}
		
		[SwaggerOperation(Tags = ["2FA"])]
		[HttpPost("setup2fa")]
		[Authorize(AuthenticationSchemes = "ChallengeTokenScheme")]
		public async Task<IActionResult> CompleteSetup2FA([FromBody] TwoFACodeRequest codeRequest) {
			var username = User.Claims.First(c => c.Type == "userId").Value;
			var user = await _context.Users.Include(user => user.User2FA).FirstOrDefaultAsync(u => u.Username == username);
			if(user == null) return BadRequest();
			if (user.User2FA.Is2FAEnabled) return BadRequest("2FA is already enabled");

			var secret = user.User2FA.SecretKey;
			var otp = new TOTP(secretKey: secret, account: user.Username);
			var valid = otp.GetValidCodes().Contains(codeRequest.Code);
			
			if(!valid) return BadRequest("Invalid 2FA code.");

			user.User2FA.Is2FAEnabled = true;
			await _context.SaveChangesAsync();
			
			return Ok();
		}

		[SwaggerOperation(Tags = ["Test"])]
		[HttpGet("refreshTokenTest")]
		[Authorize(AuthenticationSchemes = "RefreshTokenScheme")]
		public IActionResult RefreshTokenTest() {
			return Ok("Valid Refresh Token.");
		}

		[SwaggerOperation(Tags = ["Test"])]
		[HttpGet("newRefreshToken")]
		[Authorize]
		public IActionResult NewRefreshToken() {
			try {
				var username = User.Claims.First(c => c.Type == "userId").Value;
				return Ok(_jwtService.GenerateRefreshToken(username));
			} catch (InvalidOperationException e) {
				return BadRequest();
			}
		}
		
		[SwaggerOperation(Tags = ["Test"])]
		[HttpGet("newChallengeToken")]
		[Authorize]
		public IActionResult NewChallengeToken() {
			try {
				var username = User.Claims.First(c => c.Type == "userId").Value;
				return Ok(_jwtService.GenerateChallengeToken(username));
			} catch (InvalidOperationException e) {
				return BadRequest();
			}
		}
		
		[SwaggerOperation(Tags = ["2FA"])]
		[HttpPost("recover2fa")]
		[Authorize(AuthenticationSchemes = "ChallengeTokenScheme")]
		public async Task<IActionResult> Recovery2FA([FromBody] Recovery2FARequest recovery2FaRequest) {
			if (!ModelState.IsValid) {
				return BadRequest(ModelState);
			}

			User user;
			try {
				user = _userService.Login(recovery2FaRequest.UserAuthRequest);
				await _userService.Revoke(user.Username);
			} catch (UserAuthenticationException e) {
				return BadRequest(e.Message);
			}
			if (!user.User2FA.Is2FAEnabled) return BadRequest("2FA is not enabled");

			foreach (var backupCode in user.User2FA.BackupCodes) {
				if (!BCrypt.Net.BCrypt.Verify(recovery2FaRequest.Code, backupCode.HashedCode)) continue;
				backupCode.IsUsed = true;
				user.User2FA.Is2FAEnabled = false;
				await _context.SaveChangesAsync();
				//TODO: reset 2fa instead of disabling it 
				return Ok("2FA disabled!");
			}
			
			return BadRequest("Invalid Backup Code");
		}
        
		[SwaggerOperation(Tags = ["Test"])]
		[HttpGet("challengeTokenTest")]
		[Authorize(AuthenticationSchemes = "ChallengeTokenScheme")]
		public IActionResult ChallengeTokenTest() {
			return Ok("Valid Challenge Token.");
		}

	}

}
