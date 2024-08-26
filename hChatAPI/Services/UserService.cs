using System.Security.Cryptography;
using hChatAPI.Interfaces;
using hChatAPI.Models;
using hChatAPI.Models.Requests;
using hChatAPI.Models.Responses;
using hChatAPI.Services._2FA;
using hChatShared;
using Microsoft.EntityFrameworkCore;
using HashAlgorithm = hChatAPI.Services._2FA.HashAlgorithm;

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
				PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password, 12),
				User2FA = new User2FA()
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
			var dummyHash = "$2a$12$Nu5.aVWnLDLZfwJHuDQMveUVjpu.zSI4ECFYDvlqeGxtueLmfkNJ.";
			var hashToVerify = user?.PasswordHash ?? dummyHash;
			
			if (!BCrypt.Net.BCrypt.Verify(request.Password, hashToVerify)) {
				throw new UserAuthenticationException("Invalid username or password");
			}

			if (user == null) {
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

		public async Task<User2FASetupResponse> Setup2FA(string userId) {
			var user = await _context.Users.Include(u=> u.User2FA).ThenInclude(u=>u.BackupCodes).FirstOrDefaultAsync(u => u.Username == userId);
			if (user == null) {
				throw new InvalidOperationException("User not found in DB");
			}

			var totp = new TOTP(HashAlgorithm.SHA256, account: user.Username);
			
			//TODO: Encrypt using AES-256
			user.User2FA.SecretKey = totp.SecretKey;

			var codes = new string[5];
			
			using (var rng = RandomNumberGenerator.Create()) {
				for (int i = 0; i < 5; i++) {
					var bytes = new byte[16];
					rng.GetBytes(bytes);
					var code = Base32Encoder.Encode(bytes);
					codes[i] = code;
					var backupCode = new UserBackupCode {
						HashedCode = BCrypt.Net.BCrypt.HashPassword(code, 12),
						IsUsed = false
					};
					user.User2FA.BackupCodes.Add(backupCode);
				}
			}

			var response = new User2FASetupResponse {
				Uri = totp.URI,
				BackupCodes = codes
			};

			await _context.SaveChangesAsync();
			
			return response;
		}
	}


	
	public class UserRegistrationException(string message) : Exception(message);

	public class UserAuthenticationException(string message) : Exception(message);

}

