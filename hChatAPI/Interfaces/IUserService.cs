using hChatAPI.Models;
using hChatAPI.Models.Requests;
using hChatAPI.Models.Responses;

namespace hChatAPI.Interfaces {
	public interface IUserService {
		public User Register(UserAuthRequest request);
		public User Login(UserAuthRequest request);
		public Task Revoke(string userId);
		public Task<User2FASetupResponse> Setup2FA(string userId);
		
	}
}
