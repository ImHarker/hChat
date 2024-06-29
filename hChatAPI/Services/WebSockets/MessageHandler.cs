using System.Collections;
using System.IO;
using System.Net.WebSockets;
using System.Reflection.PortableExecutable;
using System.Text;
using hChatAPI.Net;

namespace hChatAPI.Services.WebSockets {
	public static class MessageHandler {

		public static async void HandleMessage(WebSocket socket, byte[] buffer) {

			using MemoryStream stream = new MemoryStream(buffer);
				using var packetReader = new PacketReader(stream);
				var opCode = packetReader.ReadOpCode();


				switch (opCode) {
					case 0:
						
						break;
					case 1:
						Console.WriteLine(packetReader.ReadMessage());
						Console.WriteLine(packetReader.ReadMessage());
						var packet = new PacketBuilder();
						packet.WriteOpCode(1);
						packet.WriteMessage("Hello from server!");
						var messageBytes = packet.GetPacketBytes();
						await socket.SendAsync(new ArraySegment<byte>(messageBytes), WebSocketMessageType.Text, true, CancellationToken.None);

					await socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "GoodBye!", CancellationToken.None);
						break;
					default:
						Console.WriteLine($"Invalid opcode received. {opCode}");
						break;
				}


		}


	}
}
