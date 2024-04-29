using hChatAPI.Models.Requests;
using hChatAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Cryptography;
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
				return Ok(_jwtService.GenerateToken(user.Username));
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
		public IActionResult Revoke() {
			try {
				_userService.Revoke(User.Claims.First(c => c.Type == "userId").Value);
			}
			catch (InvalidOperationException e) {
				return BadRequest();
			}
			return Ok("Revoked tokens.");
		}



	}

}
