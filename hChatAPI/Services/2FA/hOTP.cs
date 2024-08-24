using System.Security.Cryptography;
using hChatShared;

namespace hChatAPI.Services._2FA {
	
	public enum HashAlgorithm {
		SHA1,
		SHA256,
		SHA512
	}

	public enum Digits {
		Six = 6,
		Eight = 8
	}
	public enum Period {
		Thirty = 30,
		Sixty = 60
	}

	public class TOTP {
		public HashAlgorithm Algorithm { get; }
		private Period Period { get; }
		private Digits Digits { get; }
		private string? Account { get; }
		private string? Issuer { get; }
		private string? SecretKey { get; }
		public string URI { get; }

		private HMAC? HMAC { get; }

		public TOTP(HashAlgorithm algorithm = HashAlgorithm.SHA256, string? secretKey = null, Period period = Period.Thirty, Digits digits = Digits.Eight, string? issuer = "hChat", string? account = null) {
			Algorithm = algorithm;
			Period = period;
			Digits = digits;
			Account = account;
			Issuer = issuer;
			SecretKey = secretKey;

			HMAC = algorithm switch {
				HashAlgorithm.SHA1 => new HMACSHA1(),
				HashAlgorithm.SHA256 => new HMACSHA256(),
				HashAlgorithm.SHA512 => new HMACSHA512(),
				_ => throw new NotSupportedException("Unsupported hash algorithm")
			};

			if (SecretKey != null) {
				HMAC.Key = Base32Encoder.Decode(SecretKey);
			}
			else {
				SecretKey = Base32Encoder.Encode(HMAC.Key);
			}

			URI = $"otpauth://totp/{Uri.EscapeDataString(Issuer ??= "")}:{Uri.EscapeDataString(Account ??= "")}" +
			      $"?secret={SecretKey}&issuer={Uri.EscapeDataString(Issuer ??= "")}&algorithm={algorithm}&digits={(int)digits}&period={(long)Period}";
		}

		public List<string> GetValidCodes() {
			if (HMAC == null) throw new NullReferenceException("HMAC is null");

			var validCodes = new List<string>();
			long timeStep = (long)Period;
			long unixTimestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
			long timeCounter = unixTimestamp / timeStep;

			for (int i = -1; i <= 1; i++) {
				validCodes.Add(GenerateCode(timeCounter + i));
			}

			return validCodes;
		}

		private string GenerateCode(long timeCounter) {
			byte[] counterBytes = BitConverter.GetBytes(timeCounter);
			if (BitConverter.IsLittleEndian)
				Array.Reverse(counterBytes);

			byte[] hash = HMAC.ComputeHash(counterBytes);

			int offset = hash[^1] & 0x0F;

			int binary = (hash[offset] & 0x7F) << 24 |
			             (hash[offset + 1] & 0xFF) << 16 |
			             (hash[offset + 2] & 0xFF) << 8 |
			             (hash[offset + 3] & 0xFF);

			int otp = binary % (int)Math.Pow(10, (int)Digits);

			return otp.ToString($"D{(int)Digits}");
		}
	}
}
