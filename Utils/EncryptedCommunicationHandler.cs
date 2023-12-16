using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace hChat.Utils
{
    public class EncryptedCommunicationHandler {

	    public ECDiffieHellman myECDH = ECDiffieHellman.Create();

	    public string ExportPublicKey() {
			return Convert.ToBase64String(myECDH.PublicKey.ExportSubjectPublicKeyInfo());
		}

		public Aes InitializeAes(byte[] otherPartyPublicKey) {
			ECDiffieHellman otherDiffieHellman = ECDiffieHellman.Create();
			otherDiffieHellman.ImportSubjectPublicKeyInfo(otherPartyPublicKey, out _);
			Aes aes = Aes.Create();
			aes.Key = myECDH.DeriveKeyMaterial(otherDiffieHellman.PublicKey);
			aes.GenerateIV();
			aes.Mode = CipherMode.CFB;
			aes.Padding = PaddingMode.None;

			return aes;
		}

		public static byte[] EncryptData(byte[] data, SymmetricAlgorithm symmetricAlgorithm)
        {
			using (ICryptoTransform encryptor = symmetricAlgorithm.CreateEncryptor()) {
				byte[] iv = symmetricAlgorithm.IV;
				byte[] encryptedData = encryptor.TransformFinalBlock(data, 0, data.Length);
				return iv.Concat(encryptedData).ToArray();
			}
		}

        public static byte[] DecryptData(byte[] data, SymmetricAlgorithm symmetricAlgorithm)
        {
			byte[] iv = data.Take(symmetricAlgorithm.BlockSize / 8).ToArray();
			symmetricAlgorithm.IV = iv;

			using (ICryptoTransform decryptor = symmetricAlgorithm.CreateDecryptor()) {
				return decryptor.TransformFinalBlock(data, iv.Length, data.Length - iv.Length);
			}
		}

        public static byte[] SignData(byte[] data, ECDiffieHellman ecdh)
        {
            using (ECDsa ecdsa = ECDsa.Create(ecdh.ExportParameters(true)))
            {
                return ecdsa.SignData(data, HashAlgorithmName.SHA256);
            }
        }

		public static bool VerifySignature(byte[] data, byte[] signature, byte[] publicKey) {
			ECDiffieHellman otherDiffieHellman = ECDiffieHellman.Create();
			otherDiffieHellman.ImportSubjectPublicKeyInfo(publicKey, out _);
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

