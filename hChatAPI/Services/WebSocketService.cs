using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Net.WebSockets;
using System.Security.Claims;
using System.Text;

namespace hChatAPI.Services {
	public class WebSocketMiddleware {
		private readonly RequestDelegate _next;
		private readonly string _issuer;
		private readonly string _audience;
		private readonly string _key;
		private readonly CustomSecurityTokenHandler _customTokenHandler;

		public WebSocketMiddleware(RequestDelegate next, IConfiguration configuration, CustomSecurityTokenHandler customTokenHandler) {
			_next = next;
			_issuer = configuration["Jwt:Issuer"];
			_audience = configuration["Jwt:Audience"];
			_key = configuration["Jwt:Key"];
			_customTokenHandler = customTokenHandler;
		}

		public async Task Invoke(HttpContext context) {
			if (!context.WebSockets.IsWebSocketRequest) {
				await _next(context);
				return;
			}

			var token = context.Request.Query["token"];

			if (string.IsNullOrEmpty(token)) {
				context.Response.StatusCode = StatusCodes.Status401Unauthorized;
				return;
			}

			try {
				var validationParameters = new TokenValidationParameters {
					ValidateIssuer = true,
					ValidateAudience = true,
					ValidateLifetime = true,
					ValidateIssuerSigningKey = true,
					ValidIssuer = _issuer,
					ValidAudience = _audience,
					IssuerSigningKey = new SymmetricSecurityKey(Convert.FromBase64String(_key))
				};

				ClaimsPrincipal principal = _customTokenHandler.ValidateToken(token, validationParameters, out SecurityToken validatedToken);

				var userIdClaim = (validatedToken as JwtSecurityToken)?.Claims.FirstOrDefault(c => c.Type == "userId");
				Console.WriteLine(userIdClaim!.Value);


				var socket = await context.WebSockets.AcceptWebSocketAsync();
				await HandleWebSocket(socket);
			} catch (SecurityTokenValidationException) {
				context.Response.StatusCode = StatusCodes.Status401Unauthorized;
				return;
			} catch (WebSocketException) {
				context.Response.StatusCode = StatusCodes.Status500InternalServerError;
				return;
			}
		}

		private async Task HandleWebSocket(WebSocket socket) {
			var buffer = new byte[1024 * 128];
			WebSocketReceiveResult result = await socket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

			while (!result.CloseStatus.HasValue) {
				string message = System.Text.Encoding.UTF8.GetString(buffer, 0, result.Count);
				Console.WriteLine($"Received message: {message}");

				await socket.SendAsync(new ArraySegment<byte>(buffer, 0, result.Count), result.MessageType, result.EndOfMessage, CancellationToken.None);
				Array.Clear(buffer, 0, buffer.Length);
				result = await socket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
			}

			await socket.CloseAsync(result.CloseStatus.Value, result.CloseStatusDescription, CancellationToken.None);
		}
	}
}