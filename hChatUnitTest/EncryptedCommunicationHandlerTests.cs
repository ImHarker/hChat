using System.Security.Cryptography;
using System.Text;
using hChatTermClient;
using hChatTermClient.Wrapper;

namespace hChatUnitTest
{
    [TestFixture]
    public class EncryptedCommunicationHandlerTests
    {
        private ECDiffieHellman? _myEcdh;
        private byte[] _publicKey;

        [SetUp]
        public void Setup()
        {
            // Set up the ECDH instance and generate the public key
            _myEcdh = ECDiffieHellman.Create();
            _publicKey = Convert.FromBase64String(EncryptedCommunicationHandler.ExportPublicKey());
        }
        
        [TearDown]
        public void TearDown()
        {
            // Dispose of the ECDH instance
            if (_myEcdh != null)
            {
                _myEcdh.Dispose();
            }
        }

        [Test]
        public void EncryptData_ShouldEncryptAndDecryptSuccessfully()
        {
            // Arrange
            var message = Encoding.UTF8.GetBytes("Hello, World!");
            var salt = new byte[32];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(salt);
            }

            // Act
            var encryptedData = EncryptedCommunicationHandler.EncryptData(_publicKey, message, salt);
            var decryptedData = EncryptedCommunicationHandler.DecryptData(_publicKey, encryptedData);

            // Assert
            Assert.That(message, Is.EqualTo(decryptedData));
        }

        [Test]
        public void SignData_ShouldSignAndVerifySuccessfully()
        {
            // Arrange
            var data = Encoding.UTF8.GetBytes("Signature Test");

            // Act
            var signature = EncryptedCommunicationHandler.SignData(data);
            var isValid = EncryptedCommunicationHandler.VerifySignature(data, signature, _publicKey);

            // Assert
            Assert.IsTrue(isValid);
        }

        [Test]
        public void DecryptData_WithInvalidData_ShouldThrowException()
        {
            // Arrange
            var invalidData = new byte[] { 0x00, 0x01, 0x02 }; // Invalid encrypted data

            // Act & Assert
            Assert.Throws<CryptographicException>(() =>
                EncryptedCommunicationHandler.DecryptData(_publicKey, invalidData));
        }

        [Test]
        public void VerifySignature_WithTamperedData_ShouldFail()
        {
            // Arrange
            var originalData = Encoding.UTF8.GetBytes("Original Data");
            var tamperedData = Encoding.UTF8.GetBytes("Tampered Data");

            // Act
            var signature = EncryptedCommunicationHandler.SignData(originalData);
            var isValid = EncryptedCommunicationHandler.VerifySignature(tamperedData, signature, _publicKey);

            // Assert
            Assert.IsFalse(isValid);
        }
    }
}
