using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace hChatAPI.Services {
	public class JWTService {
		public class JwtService {
			private readonly string _issuer;
			private readonly string _audience;
			private readonly string _key;

			public JwtService(IConfiguration configuration) {
				_issuer = configuration["Jwt:Issuer"];
				_audience = configuration["Jwt:Audience"];
				_key = configuration["Jwt:Key"];
			}

			public string GenerateAccessToken(string userId) {

				var tokenHandler = new JwtSecurityTokenHandler();
				var key = Convert.FromBase64String(_key);

				var tokenDescriptor = new SecurityTokenDescriptor {
					Subject = new ClaimsIdentity(new[] { new Claim("userId", userId) }),
					Expires = DateTime.UtcNow.AddHours(1),
					Issuer = _issuer,
					Audience = _audience,
					SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key),
						SecurityAlgorithms.HmacSha512Signature)
				};

				var token = tokenHandler.CreateToken(tokenDescriptor);
				return tokenHandler.WriteToken(token);
			}

			public string GenerateRefreshToken(string userId) {

				var tokenHandler = new JwtSecurityTokenHandler();
				var key = Convert.FromBase64String(_key);

				var tokenDescriptor = new SecurityTokenDescriptor {
					Subject = new ClaimsIdentity(new[] { new Claim("userId", userId) }),
					Expires = DateTime.UtcNow.AddDays(1),
					Issuer = _issuer,
					Audience = "Refresh",
					SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key),
						SecurityAlgorithms.HmacSha512Signature)
				};

				var token = tokenHandler.CreateToken(tokenDescriptor);
				return tokenHandler.WriteToken(token);
			}
			
			public string GenerateChallengeToken(string userId) {

				var tokenHandler = new JwtSecurityTokenHandler();
				var key = Convert.FromBase64String(_key);

				var tokenDescriptor = new SecurityTokenDescriptor {
					Subject = new ClaimsIdentity(new[] { new Claim("userId", userId) }),
					Expires = DateTime.UtcNow.AddMinutes(10),
					Issuer = _issuer,
					Audience = "Challenge",
					SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key),
						SecurityAlgorithms.HmacSha512Signature)
				};

				var token = tokenHandler.CreateToken(tokenDescriptor);
				return tokenHandler.WriteToken(token);
			}

		}
	}
}
