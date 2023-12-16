using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace hChat.Models {
	public class User {
		public string Username { get; set; }
		public string PublicKey { get; set; }

		public User(string username, string publicKey) {
			Username = username;
			PublicKey = publicKey;
		}
	}
}
