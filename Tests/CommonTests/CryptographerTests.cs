using Common;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace CommonTests
{
    [TestClass]
    public class CryptographerTests
    {
        private const int RsaEncryptionLength = 512;
        private const int HashLength = 32;
        private const int ThumbprintLength = 40;
        private const int IVLength = 16;
        private const int AesKeyLength = 32;

        private const string cryptoCertThumbprint1 = "f72c89070dbec2402ac73a1a107d85a85a0b6c1b"; // mca-test-1.ht
        private const string cryptoCertThumbprint2 = "2e0ff18224c0309a0ea8a45b3d74d4af506e9b6f"; // mca-test-2.ht

        private byte[] AesKey1;
        private byte[] AesKey2;
        private byte[] AesIV1;
        private byte[] AesIV2;

        private X509Certificate2 cryptoCertificate1;
        private X509Certificate2 cryptoCertificate2;

        private ICryptographer cryptographer1;
        private ICryptographer cryptographer2;
        private ICryptographer cryptographer3;
        private ICryptographer cryptographer4;

        private Mock<ILogger> logger;

        [TestInitialize]
        public void Initialize()
        {
            cryptoCertificate1 = new X509Certificate2("certs/mca-test-1.ht.pfx", "1234");
            cryptoCertificate2 = new X509Certificate2("certs/mca-test-2.ht.pfx", "1234");

            byte[] encryptedAesKey1 = Cryptographer.GenerateAesKey(cryptoCertificate1);
            byte[] encryptedAesKey2 = Cryptographer.GenerateAesKey(cryptoCertificate2);
            byte[] encryptedAesIV1 = Cryptographer.GenerateAesIV(cryptoCertificate1);
            byte[] encryptedAesIV2 = Cryptographer.GenerateAesIV(cryptoCertificate2);

            AesKey1 = RsaDecrypt(encryptedAesKey1, cryptoCertificate1);
            AesKey2 = RsaDecrypt(encryptedAesKey2, cryptoCertificate2);
            AesIV1 = RsaDecrypt(encryptedAesIV1, cryptoCertificate1);
            AesIV2 = RsaDecrypt(encryptedAesIV2, cryptoCertificate2);

            logger = new Mock<ILogger>();

            cryptographer1 = new Cryptographer(encryptedAesKey1, encryptedAesIV1, cryptoCertificate1, logger.Object);
            cryptographer2 = new Cryptographer(encryptedAesKey2, encryptedAesIV2, cryptoCertificate2, logger.Object);
            cryptographer3 = new Cryptographer(encryptedAesKey1, RsaEncrypt(AesIV2, cryptoCertificate1), cryptoCertificate1, logger.Object); // same key, different IV
            cryptographer4 = new Cryptographer(RsaEncrypt(AesKey2, cryptoCertificate1), encryptedAesIV1, cryptoCertificate1, logger.Object); // same IV, different key
        }

        [TestCleanup]
        public void Cleanup()
        {

        }

        [TestMethod]
        [TestCategory("Bvt")]
        public void AesKeyGenerationTest()
        {
            byte[] newEncryptedKey = Cryptographer.GenerateAesKey(cryptoCertificate1);
            Assert.AreEqual(RsaEncryptionLength, newEncryptedKey.Length);
            byte[] decryptedKey = RsaDecrypt(newEncryptedKey, cryptoCertificate1);
            Assert.AreEqual(AesKeyLength, decryptedKey.Length);
        }

        [TestMethod]
        [TestCategory("Bvt")]
        public void AesIVGenerationTest()
        {
            byte[] newEncryptedIV = Cryptographer.GenerateAesIV(cryptoCertificate1);
            Assert.AreEqual(RsaEncryptionLength, newEncryptedIV.Length);
            byte[] decryptedIV = RsaDecrypt(newEncryptedIV, cryptoCertificate1);
            Assert.AreEqual(IVLength, decryptedIV.Length);
        }

        [TestMethod]
        [TestCategory("Bvt")]
        public async Task RsaEncryptAndDecryptTest()
        {
            byte[] randomContent = Utils.GenerateByteArray(256);
            byte[] encryptedContent = await cryptographer1.RsaEncrypt(randomContent);
            Assert.AreEqual(RsaEncryptionLength, encryptedContent.Length);

            byte[] decryptedContent = await cryptographer1.RsaDecrypt(encryptedContent);
            Assert.IsTrue(randomContent.SequenceEqual(decryptedContent));
        }

        [TestMethod]
        [TestCategory("Bvt")]
        public async Task AesEncryptAndDecryptTest()
        {
            byte[] randomContent = Utils.GenerateByteArray(1000);
            byte[] encryptedContent = await cryptographer1.AesEncrypt(randomContent);

            byte[] decryptedContent = await cryptographer1.AesDecrypt(encryptedContent);
            Assert.IsTrue(randomContent.SequenceEqual(decryptedContent));
        }

        [TestMethod]
        [TestCategory("Bvt")]
        public async Task HashTest()
        {
            byte[] randomContent = Utils.GenerateByteArray(1000);
            byte[] hash = await cryptographer1.Hash(randomContent);
            Assert.AreEqual(HashLength, hash.Length);
            Assert.IsTrue(Hash(randomContent).SequenceEqual(hash));
        }

        [TestMethod]
        [TestCategory("Bvt")]
        public async Task RsaDecryptWithWrongCertificateTest()
        {
            byte[] randomContent = Utils.GenerateByteArray(256);
            byte[] encryptedContent = await cryptographer1.RsaEncrypt(randomContent);
            Assert.AreEqual(RsaEncryptionLength, encryptedContent.Length);

            try
            {
                byte[] decryptedContent = await cryptographer2.RsaDecrypt(encryptedContent);
                Assert.Fail("Should throw CryptographicException");
            }
            catch (Exception e)
            {
                Assert.IsTrue(e.Message.Contains("The parameter is incorrect"));
            }
        }

        [TestMethod]
        [TestCategory("Bvt")]
        public async Task AesDecryptWithWrongKeyTest()
        {
            byte[] randomContent = Utils.GenerateByteArray(1000);
            byte[] encryptedContent = await cryptographer1.AesEncrypt(randomContent);

            try
            {
                byte[] decryptedContent = await cryptographer4.AesDecrypt(encryptedContent);
                Assert.Fail("Should throw CryptographicException");
            }
            catch (CryptographicException)
            {
            }
        }

        [TestMethod]
        [TestCategory("Bvt")]
        public async Task AesDecryptWithWrongIVTest()
        {
            byte[] randomContent = Utils.GenerateByteArray(1000);
            byte[] encryptedContent = await cryptographer1.AesEncrypt(randomContent);

            byte[] decryptedContent = await cryptographer3.AesDecrypt(encryptedContent);
            Assert.IsFalse(decryptedContent.SequenceEqual(randomContent));
        }

        [TestMethod]
        [TestCategory("Bvt")]
        public async Task AesDecryptWithWrongKeyAndIVTest()
        {
            byte[] randomContent = Utils.GenerateByteArray(1000);
            byte[] encryptedContent = await cryptographer1.AesEncrypt(randomContent);

            try
            {
                byte[] decryptedContent = await cryptographer2.AesDecrypt(encryptedContent);
                Assert.Fail("Should throw CryptographicException");
            }
            catch (CryptographicException)
            {
            }
        }

        private byte[] RsaDecrypt(byte[] encryptedContent, X509Certificate2 cert)
        {
            using (RSA rsa = cert.GetRSAPrivateKey())
            {
                return rsa.Decrypt(encryptedContent, RSAEncryptionPadding.OaepSHA1);
            }
        }

        private byte[] RsaEncrypt(byte[] content, X509Certificate2 cert)
        {
            using (RSA rsa = cert.GetRSAPublicKey())
            {
                return rsa.Encrypt(content, RSAEncryptionPadding.OaepSHA1);
            }
        }

        private byte[] Hash(byte[] content)
        {
            using (SHA256 sha = SHA256.Create())
            {
                return sha.ComputeHash(content);
            }
        }
    }
}
