﻿
using System.Net;
using System.Net.Http.Headers;
using System.Net.WebSockets;
using System.Text;
using hChatAPI.Net;
using hChatTermClient.Wrapper;

namespace hChatTermClient {
	public class Program {
		static async Task Main(string[] args) {
			var client = new Client();
			await client.Run();
			var packet = new PacketBuilder();
			packet.WriteOpCode(0x03);
			packet.WriteMessage("admin");
			packet.WriteMessage("Mensagem random pra teste");
			await client.Send(packet);
			Console.ReadKey();

		}
	}
}
