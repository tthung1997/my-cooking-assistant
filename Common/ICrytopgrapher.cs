﻿using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace Common
{
    public interface ICryptographer
    {
        Task<byte[]> AesEncrypt(byte[] content);
        Task<byte[]> AesDecrypt(byte[] encryptedContent);
        Task<byte[]> RsaEncrypt(byte[] content);
        Task<byte[]> RsaDecrypt(byte[] encryptedContent);
        Task<byte[]> Hash(byte[] content);
    }

    public class Cryptographer : ICryptographer
    {
        private const int RsaEncryptionLength = 256;
        private const int HashLength = 32;
        private const int ThumbprintLength = 40;
        private const int IVLength = 16;
        private const int AesKeyLength = 128;

        private readonly string loggingSource;
        private readonly ILogger logger;

        private readonly byte[] aesKey;
        private readonly byte[] aesIV;
        private readonly X509Certificate2 rsaCert;

        public Cryptographer(byte[] encryptedAesKey, byte[] encryptedAesIV, X509Certificate2 rsaCert, ILogger logger, string loggingSource = "Cryptographer")
        {
            if (logger == null)
            {
                throw new ArgumentNullException(nameof(logger));
            }
            this.loggingSource = loggingSource;
            this.logger = logger;

            if (rsaCert == null)
            {
                throw new ArgumentNullException(nameof(rsaCert));
            }
            this.rsaCert = rsaCert;

            encryptedAesKey.IsNullOrEmpty().Throws(new ArgumentNullException(nameof(encryptedAesKey)), logger, loggingSource);
            encryptedAesIV.IsNullOrEmpty().Throws(new ArgumentNullException(nameof(encryptedAesIV)), logger, loggingSource);
            aesKey = RsaDecrypt(encryptedAesKey).Result;
            aesIV = RsaDecrypt(encryptedAesIV).Result;
        }

        public async Task<byte[]> AesDecrypt(byte[] encryptedContent)
        {
            encryptedContent.IsNullOrEmpty().Throws(new ArgumentNullException(nameof(encryptedContent)), logger, loggingSource);
            var aes = Aes.Create();
            await logger.LogInfo(loggingSource, $"Creating AES decryptor for decryption");
            var decryptor = aes.CreateDecryptor(aesKey, aesIV);
            await logger.LogInfo(loggingSource, $"Start decrypting content of length {encryptedContent.Length} { (encryptedContent.Length > 1).ToPlural("byte") } using AES");
            var decryptedContent = decryptor.TransformFinalBlock(encryptedContent, 0, encryptedContent.Length);
            await logger.LogInfo(loggingSource, $"Finished decrypting. The decrypted text is {decryptedContent.Length} { (decryptedContent.Length > 1).ToPlural("byte") }");
            return decryptedContent;
        }

        public async Task<byte[]> AesEncrypt(byte[] content)
        {
            content.IsNullOrEmpty().Throws(new ArgumentNullException(nameof(content)), logger, loggingSource);
            var aes = Aes.Create();
            await logger.LogInfo(loggingSource, $"Creating AES encryptor for encryption");
            var encryptor = aes.CreateEncryptor(aesKey, aesIV);
            await logger.LogInfo(loggingSource, $"Start encrypting content of length {content.Length} { (content.Length > 1).ToPlural("byte") } using AES");
            var encryptedContent = encryptor.TransformFinalBlock(content, 0, content.Length);
            await logger.LogInfo(loggingSource, $"Finished encrypting. The cipher is {encryptedContent.Length} { (encryptedContent.Length > 1).ToPlural("byte") }");
            return encryptedContent;
        }

        public async Task<byte[]> Hash(byte[] content)
        {
            content.IsNullOrEmpty().Throws(new ArgumentNullException(nameof(content)), logger, loggingSource);
            await logger.LogInfo(loggingSource, $"Start hasing content of length {content.Length} { (content.Length > 1).ToPlural("byte") }");
            var sha256 = SHA256.Create();
            var hashedContent = sha256.ComputeHash(content);
            await logger.LogInfo(loggingSource, $"Finished hasing. The content hash is {hashedContent.Length} { (hashedContent.Length > 1).ToPlural("byte") }");
            return hashedContent;
        }

        public async Task<byte[]> RsaDecrypt(byte[] encryptedContent)
        {
            encryptedContent.IsNullOrEmpty().Throws(new ArgumentNullException(nameof(encryptedContent)), logger, loggingSource);
            using (var rsa = rsaCert.GetRSAPrivateKey())
            {
                await logger.LogInfo(loggingSource, $"Start decrypting content of length {encryptedContent.Length} { (encryptedContent.Length > 1).ToPlural("byte") } using RSA.");
                var decryptedContent = rsa.Decrypt(encryptedContent, RSAEncryptionPadding.OaepSHA1);
                await logger.LogInfo(loggingSource, $"Finished decrypting. The decrypted text is {decryptedContent.Length} { (decryptedContent.Length > 1).ToPlural("byte") }");
                return decryptedContent;
            }
        }

        public async Task<byte[]> RsaEncrypt(byte[] content)
        {
            content.IsNullOrEmpty().Throws(new ArgumentNullException(nameof(content)), logger, loggingSource);
            using (var rsa = rsaCert.GetRSAPublicKey())
            {
                await logger.LogInfo(loggingSource, $"Start encrypting content of length {content.Length} { (content.Length > 1).ToPlural("byte") } using RSA.");
                var encryptedContent = rsa.Encrypt(content, RSAEncryptionPadding.OaepSHA1);
                await logger.LogInfo(loggingSource, $"Finished encrypting. The cipher is {encryptedContent.Length} { (encryptedContent.Length > 1).ToPlural("byte") }");
                return encryptedContent;
            }
        }
    }
}
