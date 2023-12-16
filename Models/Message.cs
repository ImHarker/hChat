using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using hChat.Utils;
using HLogger;

namespace hChat.Models {
	public class Message {
		public User Sender { get; set; }
		public string Data { get; set; }
		public string? Signature { get; set; }
		public DateTime Timestamp { get; set; }


		public Message() {}

		public Message(string message) {
			Sender = ClientWrapper.User;
			Data = message;

			Timestamp = DateTime.Now;
		}

		public string Serialize() {
			return JsonSerializer.Serialize(this);
		}

		public static Message Deserialize(string message) {
			return JsonSerializer.Deserialize<Message>(message);
		}


	}
}
