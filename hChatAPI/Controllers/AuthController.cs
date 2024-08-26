using hChatAPI.Models.Requests;
using hChatAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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

		[HttpGet("refreshTokenTest")]
		[Authorize(AuthenticationSchemes = "RefreshTokenScheme")]
		public async Task<IActionResult> RefreshTokenTest() {
			return Ok("Valid Refresh Token.");
		}

		[HttpGet("newRefreshToken")]
		[Authorize]
		public async Task<IActionResult> NewRefreshToken() {
			try {
				var username = User.Claims.First(c => c.Type == "userId").Value;
				return Ok(_jwtService.GenerateRefreshToken(username));
			} catch (InvalidOperationException e) {
				return BadRequest();
			}
		}
		
		[HttpGet("newChallengeToken")]
		[Authorize]
		public async Task<IActionResult> NewChallengeToken() {
			try {
				var username = User.Claims.First(c => c.Type == "userId").Value;
				return Ok(_jwtService.GenerateChallengeToken(username));
			} catch (InvalidOperationException e) {
				return BadRequest();
			}
		}
		

		[HttpGet("challengeTokenTest")]
		[Authorize(AuthenticationSchemes = "ChallengeTokenScheme")]
		public async Task<IActionResult> ChallengeTokenTest() {
			return Ok("Valid Challenge Token.");
		}

	}

}
