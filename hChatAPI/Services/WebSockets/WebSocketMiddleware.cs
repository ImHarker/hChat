using System.Collections.Concurrent;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Net.WebSockets;
using System.Security.Claims;
using System.Text;

namespace hChatAPI.Services.WebSockets {
	public class WebSocketMiddleware {
		private readonly RequestDelegate _next;
		private readonly string? _issuer;
		private readonly string? _audience;
		private readonly string? _key;
		private readonly CustomSecurityTokenHandler _customTokenHandler;
		private readonly IServiceScopeFactory _serviceScopeFactory;

		
		private ConcurrentDictionary<string, WebSocket> _sockets = new();

		public WebSocketMiddleware(RequestDelegate next, IConfiguration configuration, CustomSecurityTokenHandler customTokenHandler, IServiceScopeFactory serviceScopeFactory) {
			_next = next;
			_issuer = configuration["Jwt:Issuer"];
			_audience = configuration["Jwt:Audience"];
			_key = configuration["Jwt:Key"];
			_customTokenHandler = customTokenHandler;
			_serviceScopeFactory = serviceScopeFactory;
		}

		public async Task Invoke(HttpContext context) {
			if (!context.WebSockets.IsWebSocketRequest) {
				await _next(context);
				return;
			}

			var token = context.Request.Query["token"];

			if (string.IsNullOrEmpty(token)) {
				await HandleUnauthorizedRequest(context);
				return;
			}

			try
			{
				var validationParameters = new TokenValidationParameters
				{
					ValidateIssuer = true,
					ValidateAudience = true,
					ValidateLifetime = true,
					ValidateIssuerSigningKey = true,
					ValidIssuer = _issuer,
					ValidAudience = _audience,
					IssuerSigningKey = new SymmetricSecurityKey(Convert.FromBase64String(_key))
				};

				ClaimsPrincipal principal = _customTokenHandler.ValidateToken(token, validationParameters, out SecurityToken validatedToken);

				var userId = (validatedToken as JwtSecurityToken)?.Claims.FirstOrDefault(c => c.Type == "userId")!.Value!;
				Console.WriteLine(userId);

				var socket = await context.WebSockets.AcceptWebSocketAsync();
				_sockets.TryAdd(userId, socket);
				await HandleWebSocket(userId, socket);
			}
			catch (SecurityTokenValidationException)
			{
				await HandleUnauthorizedRequest(context);
			}
            catch (WebSocketException)
            {
                await HandleWebSocketError(context);
            }
        }

		private async Task HandleUnauthorizedRequest(HttpContext context) {
			if (!context.Response.HasStarted) {
				context.Response.StatusCode = StatusCodes.Status401Unauthorized;
				await context.Response.WriteAsync("Unauthorized request");
			}
		}

		private async Task HandleWebSocketError(HttpContext context) {
			if (!context.Response.HasStarted) {
				context.Response.StatusCode = StatusCodes.Status500InternalServerError;
				await context.Response.WriteAsync("WebSocket error");
			}
		}

		private async Task HandleWebSocket(string userId, WebSocket socket)
		{
			var buffer = new byte[1024 * 128];
			
			using var scope = _serviceScopeFactory.CreateScope();
			var scopedServiceProvider = scope.ServiceProvider;
			var scopedDataContext = scopedServiceProvider.GetRequiredService<DataContext>();
			
			try
			{
				WebSocketReceiveResult result = await socket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

				while (!result.CloseStatus.HasValue)
				{
					if (result.EndOfMessage)
					{
						MessageHandler.HandleMessage(userId, socket, buffer, _sockets, scopedDataContext);
						Array.Clear(buffer, 0, buffer.Length);
					}

					result = await socket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
				}

				_sockets.TryRemove(userId, out _);	
				await socket.CloseAsync(result.CloseStatus.Value, result.CloseStatusDescription, CancellationToken.None);
			}
			catch (WebSocketException)
			{
				_sockets.TryRemove(userId, out _);
				if (socket.State != WebSocketState.Closed) { 
					await socket.CloseAsync(WebSocketCloseStatus.InternalServerError, "WebSocket connection closed unexpectedly", CancellationToken.None);
				}
				throw;
			}
		}
	}
}
