using System.ComponentModel.DataAnnotations;

namespace hChatAPI.Models.Requests {
	public class UserAuthRequest {
		[Required]
		public string Username { get; set; } = "";
		[Required]
		public string Password { get; set; } = "";
	}
}
