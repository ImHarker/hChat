﻿namespace hChatAPI.Models {
	public class User {
		public int Id { get; set; }
		public string Username { get; set; } = "";
		public string PasswordHash { get; set; } = "";
		public string PubKey { get; set; } = "";
		public DateTime RevocationTime { get; set; } = DateTime.MinValue;
	}
}
