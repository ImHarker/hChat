
using System.Net;
using System.Net.Http.Headers;
using System.Net.WebSockets;
using System.Text;
using hChatAPI.Net;

namespace hChatTermClient {
	public class Program {
		static async Task Main(string[] args) {
			HttpClientHandler handler = new HttpClientHandler {
				ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => true
			};
			HttpClient client = new HttpClient(handler);
			var url = "https://localhost:7168/";
			var response = await client.GetAsync(url + "status");
			Console.WriteLine(response.Content.ReadAsStringAsync().Result);

			response = await client.PostAsync(url + "auth/login", new StringContent("{\"username\":\"admin\",\"password\":\"admin123!\"}", Encoding.UTF8, "application/json"));
			Console.WriteLine(response.Content.ReadAsStringAsync().Result);

			var token = response.Content.ReadAsStringAsync().Result;
			client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

			response = await client.GetAsync(url + "Auth/protected");
			Console.WriteLine(response.Content.ReadAsStringAsync().Result);




			string serverUri = "wss://localhost:7168/ws?token=" + token; // Ensure the server is running and accessible

			using (ClientWebSocket socket = new ClientWebSocket()) {
				// Bypass SSL certificate validation for local testing (not recommended for production)
				socket.Options.RemoteCertificateValidationCallback = (sender, certificate, chain, sslPolicyErrors) => true;

				try {
					Console.WriteLine("Connecting to the server...");
					await socket.ConnectAsync(new Uri(serverUri), CancellationToken.None);
					Console.WriteLine("Connected to the server");

					// Send a message to the server
					PacketBuilder packet = new PacketBuilder();
					packet.WriteOpCode(1);
					packet.WriteMessage("Hello WebSocket Server!");
					packet.WriteMessage("This is a test!");
					byte[] messageBytes = packet.GetPacketBytes();
					await socket.SendAsync(new ArraySegment<byte>(messageBytes), WebSocketMessageType.Text, true, CancellationToken.None);
					Console.WriteLine($"Sent: {Encoding.UTF8.GetString(packet.GetPacketBytes())}");

					// Receive a message from the server
					byte[] buffer = new byte[1024];
					WebSocketReceiveResult result = await socket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
					string receivedMessage = Encoding.UTF8.GetString(buffer, 0, result.Count);
					Console.WriteLine($"Received: {receivedMessage}");
				} catch (WebSocketException e) {
					Console.WriteLine($"WebSocket error: {e.Message}");
					Console.WriteLine($"WebSocket error: {e.InnerException?.Message}");
				} catch (Exception e) {
					Console.WriteLine($"Unexpected error: {e.Message}");
				} finally {
					if (socket.State == WebSocketState.Open) {
						await socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing", CancellationToken.None);
						Console.WriteLine("Closed WebSocket connection");
					}
				}
			}



			Console.ReadKey();
		}
	}
}
