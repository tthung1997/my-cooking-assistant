using AzureUtils;
using Common;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Moq;
using NuGet.Frameworks;
using System;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;

namespace AzureUtilsTests
{
    [TestClass]
    public class AzureBlobStorageTests
    {
        private string storageConnectionString;
        private string containerName = "test";

        private Cryptographer cryptographer;
        private Mock<ILogger> logger;

        private TestContext testContextInstance;

        public TestContext TestContext
        {
            get
            {
                return testContextInstance;
            }
            set
            {
                testContextInstance = value;
            }
        }

        public void Initialize(string category)
        {
            logger = new Mock<ILogger>();
            storageConnectionString = GetTestParameter("storageConnectionString");

            X509Certificate2 cryptoCertificate = new X509Certificate2("certs/mca-test.ht.pfx", "1234");
            byte[] encryptedAesKey = Cryptographer.GenerateAesKey(cryptoCertificate);
            byte[] encryptedAesIV = Cryptographer.GenerateAesIV(cryptoCertificate);
            cryptographer = new Cryptographer(encryptedAesKey, encryptedAesIV, cryptoCertificate, logger.Object);

            // remove existing container with the same name
            if (category == "DevOnly")
            {
                if (CloudStorageAccount.TryParse(storageConnectionString, out CloudStorageAccount storageAccount))
                {
                    CloudBlobClient client = storageAccount.CreateCloudBlobClient();
                    CloudBlobContainer container = client.GetContainerReference(containerName);
                    container.DeleteIfExistsAsync().Wait();
                }
            }
        }

        public void CleanUp(string category)
        {
            // remove existing container with the same name
            if (category == "DevOnly")
            {
                if (CloudStorageAccount.TryParse(storageConnectionString, out CloudStorageAccount storageAccount))
                {
                    CloudBlobClient client = storageAccount.CreateCloudBlobClient();
                    CloudBlobContainer container = client.GetContainerReference(containerName);
                    container.DeleteIfExistsAsync().Wait();
                }
            }
        }

        [TestMethod]
        [TestCategory("Bvt")]
        public void InvalidConnectionStringTest()
        {
            Initialize("Bvt");
            // null connection string
            try
            {
                AzureBlobStorage storage = new AzureBlobStorage(null, containerName, cryptographer, logger.Object);
                Assert.Fail("Should fail because connection string is null");
            }
            catch (ArgumentNullException ane)
            {
                Assert.AreEqual("Value cannot be null. (Parameter 'connectionString')", ane.Message);
            }

            // empty connection string
            try
            {
                AzureBlobStorage storage = new AzureBlobStorage("", containerName, cryptographer, logger.Object);
                Assert.Fail("Should fail because connection string is empty");
            }
            catch (ArgumentNullException ane)
            {
                Assert.AreEqual("Value cannot be null. (Parameter 'connectionString')", ane.Message);
            }

            // random connection string
            try
            {
                AzureBlobStorage storage = new AzureBlobStorage("random connection string", containerName, cryptographer, logger.Object);
                Assert.Fail("Should fail because connection string is invalid");
            }
            catch (ArgumentException ae)
            {
                Assert.AreEqual("Failed to parse connection string to storage account", ae.Message);
            }
            CleanUp("Bvt");
        }

        [TestMethod]
        [TestCategory("Bvt")]
        public void NullContainerNameTest()
        {
            Initialize("Bvt");
            // null container name
            try
            {
                AzureBlobStorage storage = new AzureBlobStorage(storageConnectionString, null, cryptographer, logger.Object);
                Assert.Fail("Should fail because container name is null");
            }
            catch (ArgumentNullException ane)
            {
                Assert.AreEqual("Value cannot be null. (Parameter 'containerName')", ane.Message);
            }

            // empty container name
            try
            {
                AzureBlobStorage storage = new AzureBlobStorage(storageConnectionString, "", cryptographer, logger.Object);
                Assert.Fail("Should fail because container name is empty");
            }
            catch (ArgumentNullException ane)
            {
                Assert.AreEqual("Value cannot be null. (Parameter 'containerName')", ane.Message);
            }
            CleanUp("Bvt");
        }

        [TestMethod]
        [TestCategory("Bvt")]
        public void NullCryptographerTest()
        {
            Initialize("Bvt");
            try
            {
                AzureBlobStorage storage = new AzureBlobStorage(storageConnectionString, containerName, null, logger.Object);
                Assert.Fail("Should fail because cryptographer is null");
            }
            catch (ArgumentNullException ane)
            {
                Assert.AreEqual("Value cannot be null. (Parameter 'cryptographer')", ane.Message);
            }
            CleanUp("Bvt");
        }

        [TestMethod]
        [TestCategory("Bvt")]
        public void NullLoggerTest()
        {
            Initialize("Bvt");
            try
            {
                AzureBlobStorage storage = new AzureBlobStorage(storageConnectionString, containerName, cryptographer, null);
                Assert.Fail("Should fail because logger is null");
            }
            catch (ArgumentNullException ane)
            {
                Assert.AreEqual("Value cannot be null. (Parameter 'logger')", ane.Message);
            }
            CleanUp("Bvt");
        }

        [TestMethod]
        [TestCategory("DevOnly")]
        public void NewContainerTest()
        {
            Initialize("DevOnly");
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(storageConnectionString);
            CloudBlobClient client = storageAccount.CreateCloudBlobClient();
            CloudBlobContainer container = client.GetContainerReference(containerName);
            Assert.IsFalse(container.ExistsAsync().Result, "Test container should not exist.");

            new AzureBlobStorage(storageConnectionString, containerName, cryptographer, logger.Object);

            Assert.IsTrue(container.ExistsAsync().Result, "Test container should be created.");
            CleanUp("DevOnly");
        }

        [TestMethod]
        [TestCategory("DevOnly")]
        public async Task UploadAndDownloadFileTest()
        {
            Initialize("DevOnly");
            AzureBlobStorage storage = new AzureBlobStorage(storageConnectionString, containerName, cryptographer, logger.Object);
            string fileName = "TestFile";
            byte[] fileContent = Utils.GenerateByteArray(1000);
            Assert.IsFalse(await storage.FileExists(fileName));
            Assert.IsTrue(await storage.UploadFile(fileName, fileContent));
            Assert.IsTrue(await storage.FileExists(fileName));
            byte[] downloadedContent = await storage.DownloadFile(fileName);
            Assert.IsTrue(downloadedContent.SequenceEqual(fileContent));
            CleanUp("DevOnly");
        }

        [TestMethod]
        [TestCategory("DevOnly")]
        public async Task UploadInvalidFileNameTest()
        {
            Initialize("DevOnly");
            AzureBlobStorage storage = new AzureBlobStorage(storageConnectionString, containerName, cryptographer, logger.Object);
            byte[] fileContent = Utils.GenerateByteArray(1000);
            Assert.IsFalse(await storage.UploadFile(null, fileContent));
            Assert.IsFalse(await storage.UploadFile("", fileContent));
            CleanUp("DevOnly");
        }

        [TestMethod]
        [TestCategory("DevOnly")]
        public async Task UploadInvalidContentTest()
        {
            Initialize("DevOnly");
            AzureBlobStorage storage = new AzureBlobStorage(storageConnectionString, containerName, cryptographer, logger.Object);
            string fileName = "TestFile";
            Assert.IsFalse(await storage.UploadFile(fileName, null));
            Assert.IsFalse(await storage.UploadFile(fileName, new byte[0]));
            CleanUp("DevOnly");
        }

        [TestMethod]
        [TestCategory("DevOnly")]
        public async Task DownloadNotExistFileTest()
        {
            Initialize("DevOnly");
            AzureBlobStorage storage = new AzureBlobStorage(storageConnectionString, containerName, cryptographer, logger.Object);
            string fileName = "TestFile";
            Assert.IsFalse(await storage.FileExists(fileName));
            byte[] downloadedContent = await storage.DownloadFile(fileName);
            Assert.AreEqual(0, downloadedContent.Length);
            CleanUp("DevOnly");
        }

        [TestMethod]
        [TestCategory("DevOnly")]
        public async Task DownloadInvalidFileNameTest()
        {
            Initialize("DevOnly");
            AzureBlobStorage storage = new AzureBlobStorage(storageConnectionString, containerName, cryptographer, logger.Object);
            // null file name
            byte[] downloadedContent = await storage.DownloadFile(null);
            Assert.AreEqual(0, downloadedContent.Length);

            // empty file name
            downloadedContent = await storage.DownloadFile("");
            Assert.AreEqual(0, downloadedContent.Length);
            CleanUp("DevOnly");
        }

        [TestMethod]
        [TestCategory("DevOnly")]
        public async Task FileExistsTest()
        {
            Initialize("DevOnly");
            AzureBlobStorage storage = new AzureBlobStorage(storageConnectionString, containerName, cryptographer, logger.Object);
            string fileName = "TestFile";
            byte[] fileContent = Utils.GenerateByteArray(1000);
            Assert.IsFalse(await storage.FileExists(fileName));
            Assert.IsTrue(await storage.UploadFile(fileName, fileContent));
            Assert.IsTrue(await storage.FileExists(fileName));
            CleanUp("DevOnly");
        }

        [TestMethod]
        [TestCategory("DevOnly")]
        public async Task FileExistsWithInvalidFileNameTest()
        {
            Initialize("DevOnly");
            AzureBlobStorage storage = new AzureBlobStorage(storageConnectionString, containerName, cryptographer, logger.Object);
            // null file name
            Assert.IsFalse(await storage.FileExists(null));

            // empty file name
            Assert.IsFalse(await storage.FileExists(""));
            CleanUp("DevOnly");
        }

        [TestMethod]
        [TestCategory("DevOnly")]
        public async Task DeleteExistFileTest()
        {
            Initialize("DevOnly");
            AzureBlobStorage storage = new AzureBlobStorage(storageConnectionString, containerName, cryptographer, logger.Object);
            string fileName = "TestFile";
            byte[] fileContent = Utils.GenerateByteArray(1000);
            Assert.IsTrue(await storage.UploadFile(fileName, fileContent));
            Assert.IsTrue(await storage.FileExists(fileName));
            Assert.IsTrue(await storage.DeleteFile(fileName));
            Assert.IsFalse(await storage.FileExists(fileName));
            CleanUp("DevOnly");
        }

        [TestMethod]
        [TestCategory("DevOnly")]
        public async Task DeleteNotExistFileTest()
        {
            Initialize("DevOnly");
            AzureBlobStorage storage = new AzureBlobStorage(storageConnectionString, containerName, cryptographer, logger.Object);
            string fileName = "TestFile";
            Assert.IsFalse(await storage.FileExists(fileName));
            Assert.IsTrue(await storage.DeleteFile(fileName));
            CleanUp("DevOnly");
        }

        [TestMethod]
        [TestCategory("DevOnly")]
        public async Task DeleteInvalidFileNameTest()
        {
            Initialize("DevOnly");
            AzureBlobStorage storage = new AzureBlobStorage(storageConnectionString, containerName, cryptographer, logger.Object);
            // null file name
            Assert.IsTrue(await storage.DeleteFile(null));

            // empty file name
            Assert.IsTrue(await storage.DeleteFile(""));
            CleanUp("DevOnly");
        }

        private string GetTestParameter(string key)
        {
            return TestContext.Properties[key].ToString();
        }
    }
}
