using System.Security.Claims;
using hChatAPI.Interfaces;
using hChatAPI.Models;
using hChatAPI.Models.Requests;

namespace hChatAPI.Services {
	public class UserService : IUserService {
		private readonly DataContext _context;

		public UserService(DataContext context) {
			_context = context;
		}

		public User Register(UserAuthRequest request) {

			if (string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password)) {
					throw new UserRegistrationException("Username and password are required");
			}

			if (_context.Users.Any(u => u.Username == request.Username)) {
				throw new UserRegistrationException("Username is already taken");
			}

			if (!request.Username.All(char.IsLetterOrDigit)) {
				throw new UserRegistrationException("Username must be alphanumeric");
			}

			if (request.Username.Length < 5 || request.Username.Length > 32) {
				throw new UserRegistrationException("Username must be between 5 and 32 characters long");
			}

			if (request.Password.Length < 8 || request.Password.Length > 64) {
				throw new UserRegistrationException("Password must be between 8 and 64 characters long");
			}

			if (!request.Password.Any(char.IsDigit) || !request.Password.Any(char.IsLetter) || !request.Password.Any(c => char.IsSymbol(c) || char.IsPunctuation(c))) {
				throw new UserRegistrationException("Password must contain at least one letter, one number and one special character");
			}


			var user = new User {
				Username = request.Username,
				PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password, 12)
			};

			try {
				_context.Users.Add(user);
				_context.SaveChanges();
			} catch (Exception e) {
				throw new UserRegistrationException("Failed to register user. Try again later");
			}

			return user;
		}

		public User Login(UserAuthRequest request) {
			var user = _context.Users.FirstOrDefault(u => u.Username == request.Username);

			if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash)) {
				throw new UserAuthenticationException("Invalid username or password");
			}

			return user;
		}

		public async Task Revoke(string userId) {
			var user = _context.Users.FirstOrDefault(u => u.Username == userId);
			if (user == null) {
				throw new InvalidOperationException("User not found in DB");
			}
			user.RevocationTime = DateTime.UtcNow.AddSeconds(-1);
			await _context.SaveChangesAsync();
		}
	}


	
	public class UserRegistrationException : Exception {
		public UserRegistrationException(string message) : base(message) { }
	}

	public class UserAuthenticationException : Exception {
		public UserAuthenticationException(string message) : base(message) { }
	}

}

