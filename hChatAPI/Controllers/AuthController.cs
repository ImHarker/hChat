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


		public AuthController(ILogger<AuthController> logger, UserService userService, JwtService jwtService) {
			_logger = logger;
			_userService = userService;
			_jwtService = jwtService;
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
		public IActionResult Login([FromBody] UserAuthRequest request) {

			if (!ModelState.IsValid) {
				return BadRequest(ModelState);
			}

			try {
				var user = _userService.Login(request);
				return Ok(_jwtService.GenerateToken(user.Username));
			} catch (UserAuthenticationException e) {
				return BadRequest(e.Message);
			}

		}

		[HttpGet("protected")]
		[Authorize] // Requires token authentication
		public IActionResult ProtectedEndpoint() {
			return Ok("This is a protected endpoint.");
		}



	}

}
