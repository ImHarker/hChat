using System.Text;

namespace hChatTermClient.Net {
	public class PacketReader : BinaryReader {

		private MemoryStream _stream;

		public PacketReader(MemoryStream stream) : base(stream) {
			_stream = stream;
		}

		public string ReadMessage() {
			var length = ReadInt32();
			var bytes = ReadBytes(length);
			return Encoding.UTF8.GetString(bytes);
		}

		public byte ReadOpCode() {
			return ReadByte();
		}
	}
}
