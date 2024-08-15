
using hChatShared;
using hChatShared.Services;

namespace hChatTermClient {
	public class Program {
		static async Task Main(string[] args) {
			// var client = new Client();
			// await client.Run();
			// var packet = new PacketBuilder();
			// packet.WriteOpCode(0x03);
			// packet.WriteMessage("admin");
			// packet.WriteMessage("Mensagem random pra teste");
			// await client.Send(packet);
			
			Utils.CreateAppDataFolder();
			Console.WriteLine(EncryptedCommunicationHandler.ExportPublicKey());
			Console.ReadKey();

		}
	}
}
