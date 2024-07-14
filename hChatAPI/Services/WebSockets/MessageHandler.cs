using System.Collections.Concurrent;
using System.Globalization;
using System.Net.WebSockets;
using hChatAPI.Net;
using Microsoft.EntityFrameworkCore;

namespace hChatAPI.Services.WebSockets {
	
	/*
	 * OPCODE Definitions
	 * +--------+----------------------+--------------------------------------------------------------------------+
	 * | Opcode | Name                 | Description                                                              |
	 * |--------|----------------------|--------------------------------------------------------------------------|
	 * | 0x01   | REQUEST_PUBKEY       | Request public key of an user                                            |
	 * | 0x02   | SEND_PUBKEY          | Send public key to an user                                               |
	 * | 0x03   | SEND_MESSAGE         | Send a chat message                                                      |
	 * | 0x04   | RECEIVE_MESSAGE      | Receive a chat message                                                   |
	 * | 0x05   | REQUEST_MESSAGES     | Request undelivered messages (for when a user comes online)              |
	 * | 0x06   | SEND_MESSAGES        | Send undelivered messages                                                |
	 * | 0x07   | REQUEST_TIMESTAMP    | Request current timestamp from server                                    |
	 * | 0x08   | SEND_TIMESTAMP       | Send current timestamp from server                                       |
	 * +--------+----------------------+--------------------------------------------------------------------------+
	 *
	 */
	
	public static class MessageHandler {
		
		public static async void HandleMessage(string userId, WebSocket socket, byte[] buffer, ConcurrentDictionary<string, WebSocket> sockets, DataContext context) {

			using var stream = new MemoryStream(buffer);
				using var packetReader = new PacketReader(stream);
				var opCode = packetReader.ReadOpCode();


				switch (opCode) {
					
					case 0x01:
					{
						Console.WriteLine("REQUEST_PUBKEY");
						var packet = new PacketBuilder();
						packet.WriteOpCode(0x02);
						var username = packetReader.ReadMessage();
						var user =  await context.Users.FirstOrDefaultAsync(c => c.Username == username);
						packet.WriteMessage(user == null ? "" : $"{user.PubKey}");
						var messageBytes = packet.GetPacketBytes();

						await socket.SendAsync(new ArraySegment<byte>(messageBytes), WebSocketMessageType.Text, true, CancellationToken.None);
						break;
					}
					
					case 0x02:
					{ 
						Console.WriteLine("SEND_PUBKEY - Invalid Server Operation");
						break;
					}
					
					case 0x03:
					{
						Console.WriteLine("SEND_MESSAGE");
                        var user = packetReader.ReadMessage();
                        Console.WriteLine(user);
						var msg = packetReader.ReadMessage();
                        var packet = new PacketBuilder();
                        packet.WriteOpCode(0x04);
                        packet.WriteMessage($"{user} -> {msg}");
                        Console.WriteLine($"{user} -> {msg}");
                        var messageBytes = packet.GetPacketBytes();
                        sockets.TryGetValue(user, out var userSocket);
						if (userSocket != null) await userSocket.SendAsync(new ArraySegment<byte>(messageBytes), WebSocketMessageType.Text, true, CancellationToken.None);

                        break;
					}

					case 0x04:
					{
						Console.WriteLine("RECEIVE_MESSAGE - Invalid Server Operation");
						break;
					}

					case 0x05:
					{
						Console.WriteLine("REQUEST_MESSAGES");
						var packet = new PacketBuilder();
						packet.WriteOpCode(0x06);
						packet.WriteMessage("MSGS");
						break;
					}

					case 0x06:
					{
						Console.WriteLine("SEND_MESSAGES - Invalid Server Operation");
						break;
					}

					case 0x07:
					{
						Console.WriteLine("REQUEST_TIMESTAMP");
                        var packet = new PacketBuilder();
                        packet.WriteOpCode(0x08);
                        packet.WriteMessage(DateTime.UtcNow.ToString(CultureInfo.InvariantCulture));
                        var messageBytes = packet.GetPacketBytes();

                        await socket.SendAsync(new ArraySegment<byte>(messageBytes), WebSocketMessageType.Text, true, CancellationToken.None);
                        break;
					}

					case 0x08:
					{
						Console.WriteLine("SEND_TIMESTAMP - Invalid Server Operation");
						break;
					}

					default:
						Console.WriteLine($"Invalid opcode received. {opCode}");
						break;
				}


		}


	}
}
