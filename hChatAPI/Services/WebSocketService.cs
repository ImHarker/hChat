using System;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace hChatAPI.Services {
	public class WebSocketMiddleware {
		private readonly RequestDelegate _next;

		public WebSocketMiddleware(RequestDelegate next) {
			_next = next;
		}

		public async Task Invoke(HttpContext context) {
			if (!context.WebSockets.IsWebSocketRequest) {
				await _next(context);
				return;
			}

			var socket = await context.WebSockets.AcceptWebSocketAsync();
			await HandleWebSocket(socket);
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