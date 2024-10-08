﻿using System.Text;

namespace hChatShared.Net {
	public class PacketBuilder {

		MemoryStream _stream;

		public PacketBuilder() {
			_stream = new MemoryStream();
		}

		public void WriteOpCode(byte opCode) {
			_stream.WriteByte(opCode);
		}

		public void WriteMessage(string value) {
			var bytes = Encoding.UTF8.GetBytes(value);
			_stream.Write(BitConverter.GetBytes(value.Length));
			_stream.Write(bytes, 0, bytes.Length);
		}

		public byte[] GetPacketBytes() {
			return _stream.ToArray();
		}

		public void Clear() {
			_stream.SetLength(0);
			_stream.Seek(0, SeekOrigin.Begin);
		}
	}
}
