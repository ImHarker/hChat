using System.Security.Cryptography;

namespace hChatAPI.Services
{
    public static class EncryptedCommunicationHandler {
	    private static readonly ECDiffieHellman myECDH = ECDiffieHellman.Create();

	    public static string ExportPublicKey() {
			return Convert.ToBase64String(myECDH.PublicKey.ExportSubjectPublicKeyInfo());
		}

		public static Aes InitializeAes(byte[] otherPartyPublicKey, byte[] salt) {
			ECDiffieHellman otherDiffieHellman = ECDiffieHellman.Create();
			otherDiffieHellman.ImportSubjectPublicKeyInfo(otherPartyPublicKey, out _);
			var sharedSecret = myECDH.DeriveKeyMaterial(otherDiffieHellman.PublicKey);
			var key = Rfc2898DeriveBytes.Pbkdf2(sharedSecret, salt, 150_000, HashAlgorithmName.SHA256, 256 / 8);
			Aes aes = Aes.Create();
			aes.Key = key;
			aes.GenerateIV();
			aes.Mode = CipherMode.CFB;
			aes.Padding = PaddingMode.None;

			return aes;
		}

		public static byte[] EncryptData(byte[] otherPartyPublicKey, byte[] data, byte[] salt) {
			var aes = InitializeAes(otherPartyPublicKey, salt);
			using (ICryptoTransform encryptor = aes.CreateEncryptor()) {
				byte[] iv = aes.IV;
				byte[] encryptedData = encryptor.TransformFinalBlock(data, 0, data.Length);
				return salt.Concat(iv.Concat(encryptedData)).ToArray();
			}
		}

        public static byte[] DecryptData(byte[] otherPartyPublicKey, byte[] data) {
	        byte[] salt = data.Take(32).ToArray();
	        var aes = InitializeAes(otherPartyPublicKey, salt);
			byte[] iv = data.Skip(32).Take(aes.BlockSize / 8).ToArray();
			aes.IV = iv;

			using (ICryptoTransform decryptor = aes.CreateDecryptor()) {
				return decryptor.TransformFinalBlock(data, salt.Length + iv.Length, data.Length - (salt.Length + iv.Length));
			}
		}

        public static byte[] SignData(byte[] data)
        {
            using (ECDsa ecdsa = ECDsa.Create(myECDH.ExportParameters(true)))
            {
                return ecdsa.SignData(data, HashAlgorithmName.SHA256);
            }
        }

		public static bool VerifySignature(byte[] data, byte[] signature, byte[] otherPartyPublicKey) {
			ECDiffieHellman otherDiffieHellman = ECDiffieHellman.Create();
			otherDiffieHellman.ImportSubjectPublicKeyInfo(otherPartyPublicKey, out _);
			using (ECDsa ecdsa = ECDsa.Create()) {
				ECParameters parameters = new ECParameters {
					Curve = otherDiffieHellman.PublicKey.ExportParameters().Curve,
					Q = otherDiffieHellman.PublicKey.ExportParameters().Q
				};

				ecdsa.ImportParameters(parameters);

				return ecdsa.VerifyData(data, signature, HashAlgorithmName.SHA256);
			}
		}
    }
}

