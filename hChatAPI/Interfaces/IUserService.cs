using hChatAPI.Models;
using hChatAPI.Models.Requests;

namespace hChatAPI.Interfaces {
	public interface IUserService {
		public User Register(UserAuthRequest request);
		public User Login(UserAuthRequest request);
		public Task Revoke(string userId);
	}
}
