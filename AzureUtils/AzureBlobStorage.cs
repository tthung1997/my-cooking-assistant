using Common;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;

namespace AzureUtils
{
    public class AzureBlobStorage : IDataStorage
    {
        private readonly string loggingSource;
        private readonly CloudBlobContainer container;
        private readonly ILogger logger;
        private readonly ICryptographer cryptographer;

        public AzureBlobStorage(string connectionString, string containerName, ICryptographer cryptographer, ILogger logger, string loggingSource = "AzureBlobStorage")
        {
            string.IsNullOrEmpty(connectionString).Throws(new ArgumentNullException(nameof(connectionString)), logger, loggingSource);
            string.IsNullOrEmpty(containerName).Throws(new ArgumentNullException(nameof(containerName)), logger, loggingSource);
            (cryptographer == null).Throws(new ArgumentNullException(nameof(cryptographer)), logger, loggingSource);
            (logger == null).Throws(new ArgumentNullException(nameof(logger)));

            this.logger = logger;
            this.loggingSource = loggingSource;
            this.cryptographer = cryptographer;
            if (CloudStorageAccount.TryParse(connectionString, out CloudStorageAccount storageAccount))
            {
                var client = storageAccount.CreateCloudBlobClient();
                container = client.GetContainerReference(containerName);
                container.CreateIfNotExistsAsync().Wait();
            }
            else
            {
                var ex = new ArgumentException("Failed to parse connection string to storage account");
                logger.LogError(this.loggingSource, ex).Wait();
                throw ex;
            }
        }

        public async Task<bool> DeleteFile(string fileName)
        {
            try
            {
                if (await FileExists(fileName))
                {
                    await logger.LogInfo(loggingSource, $"File {fileName} exists. Start deleting.");
                    return await container.GetBlobReference(fileName).DeleteIfExistsAsync();
                }
                else
                {
                    await logger.LogInfo(loggingSource, $"File {fileName} does not exist. No action is needed.");
                    return true;
                }
            }
            catch (Exception ex)
            {
                await logger.LogError(loggingSource, ex);
                return false;
            }
        }

        public async Task<bool> FileExists(string fileName)
        {
            try
            {
                return await container.GetBlobReference(fileName).ExistsAsync();
            }
            catch (Exception ex) 
            {
                await logger.LogError(loggingSource, ex);
                return false;
            }
        }

        public async Task<byte[]> DownloadFile(string fileName)
        {
            byte[] fileContent = new byte[0];
            try
            {
                if (await FileExists(fileName))
                {
                    await logger.LogInfo(loggingSource, $"File {fileName} exists. Start downloading.");
                    var blob = container.GetBlobReference(fileName);
                    using (var ms = new MemoryStream())
                    {
                        await blob.DownloadToStreamAsync(ms);
                        var encryptedContent = ms.ToArray();
                        await logger.LogInfo(loggingSource, $"File {fileName} downloaded from blob storage. Length: {fileContent.Length} { (fileContent.Length > 1).ToPlural("byte") }");
                        await logger.LogInfo(loggingSource, $"Decrypting file {fileName}.");
                        fileContent = await cryptographer.AesDecrypt(encryptedContent);
                        await logger.LogInfo(loggingSource, $"File {fileName} decrypted. Length: {fileContent.Length} { (fileContent.Length > 1).ToPlural("byte") }");
                    }
                }
            }
            catch (Exception ex)
            {
                await logger.LogError(loggingSource, ex);
            }
            return fileContent;
        }

        public async Task<bool> UploadFile(string fileName, byte[] content)
        {
            try
            {
                await logger.LogInfo(loggingSource, $"Uploading file {fileName}");
                var blob = container.GetBlockBlobReference(fileName);
                await logger.LogInfo(loggingSource, $"Encrypting file {fileName} of {content.Length} { (content.Length > 1).ToPlural("byte") }");
                byte[] encryptedContent = await cryptographer.AesEncrypt(content);
                await blob.UploadFromByteArrayAsync(encryptedContent, 0, encryptedContent.Length);
                await logger.LogInfo(loggingSource, $"File {fileName} of {encryptedContent.Length} { (encryptedContent.Length > 1).ToPlural("byte") } uploaded to blob storage at {blob.Uri}");
                return true;
            }
            catch (Exception ex)
            {
                await logger.LogError(loggingSource, ex);
                return false;
            }
        }
    }
}
