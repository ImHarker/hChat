namespace hChatAPI.Services {
	using hChatAPI.Interfaces;
	using Microsoft.IdentityModel.Tokens;
	using System;
	using System.Collections.Generic;
	using System.IdentityModel.Tokens.Jwt;
	using System.Security.Claims;

	public class CustomSecurityTokenHandler : JwtSecurityTokenHandler {
		private readonly IServiceScopeFactory _serviceScopeFactory;

		public CustomSecurityTokenHandler(IServiceScopeFactory serviceScopeFactory) {
			_serviceScopeFactory = serviceScopeFactory;
		}


		public override ClaimsPrincipal ValidateToken(string token, TokenValidationParameters validationParameters, out SecurityToken validatedToken) {
			var standardValidationResult = base.ValidateToken(token, validationParameters, out validatedToken);

			using var scope = _serviceScopeFactory.CreateScope();
			var scopedServiceProvider = scope.ServiceProvider;
			var scopedDataContext = scopedServiceProvider.GetRequiredService<DataContext>();

			var userIdClaim = (validatedToken as JwtSecurityToken)?.Claims.FirstOrDefault(c => c.Type == "userId");
			if (userIdClaim == null) {
				throw new InvalidOperationException("User ID claim not found in token");
			}

			var userId = userIdClaim.Value;

			var user = scopedDataContext.Users.FirstOrDefault(u => u.Username == userId);
			if (user == null) {
				throw new InvalidOperationException("User not found in DB");
			}

			var globalRevocationTime = user.RevocationTime;

			var jwtSecurityToken = validatedToken as JwtSecurityToken;
			var issuedAt = jwtSecurityToken?.IssuedAt;

			if (issuedAt < globalRevocationTime) {
				throw new InvalidOperationException("Token is expired due to global revocation");
			}

			return standardValidationResult;
		}

	}

}
