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
        
        [TestInitialize]
        public void Initialize()
        {
            logger = new Mock<ILogger>();
            storageConnectionString = GetTestParameter("storageConnectionString");

            X509Certificate2 cryptoCertificate = new X509Certificate2("certs/mca-test.ht.pfx", "1234");
            byte[] encryptedAesKey = Cryptographer.GenerateAesKey(cryptoCertificate);
            byte[] encryptedAesIV = Cryptographer.GenerateAesIV(cryptoCertificate);
            cryptographer = new Cryptographer(encryptedAesKey, encryptedAesIV, cryptoCertificate, logger.Object);

            // remove existing container with the same name
            if (CloudStorageAccount.TryParse(storageConnectionString, out CloudStorageAccount storageAccount))
            {
                CloudBlobClient client = storageAccount.CreateCloudBlobClient();
                CloudBlobContainer container = client.GetContainerReference(containerName);
                container.DeleteIfExistsAsync().Wait();
            }
        }

        [TestCleanup]
        public void CleanUp()
        {
            // remove existing container with the same name
            if (CloudStorageAccount.TryParse(storageConnectionString, out CloudStorageAccount storageAccount))
            {
                CloudBlobClient client = storageAccount.CreateCloudBlobClient();
                CloudBlobContainer container = client.GetContainerReference(containerName);
                container.DeleteIfExistsAsync().Wait();
            }
        }

        [TestMethod]
        [TestCategory("Bvt")]
        public void InvalidConnectionStringTest()
        {
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
        }

        [TestMethod]
        [TestCategory("Bvt")]
        public void NullContainerNameTest()
        {
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
        }

        [TestMethod]
        [TestCategory("Bvt")]
        public void NullCryptographerTest()
        {
            try
            {
                AzureBlobStorage storage = new AzureBlobStorage(storageConnectionString, containerName, null, logger.Object);
                Assert.Fail("Should fail because cryptographer is null");
            }
            catch (ArgumentNullException ane)
            {
                Assert.AreEqual("Value cannot be null. (Parameter 'cryptographer')", ane.Message);
            }
        }

        [TestMethod]
        [TestCategory("Bvt")]
        public void NullLoggerTest()
        {
            try
            {
                AzureBlobStorage storage = new AzureBlobStorage(storageConnectionString, containerName, cryptographer, null);
                Assert.Fail("Should fail because logger is null");
            }
            catch (ArgumentNullException ane)
            {
                Assert.AreEqual("Value cannot be null. (Parameter 'logger')", ane.Message);
            }
        }

        [TestMethod]
        [TestCategory("DevOnly")]
        public void NewContainerTest()
        {
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(storageConnectionString);
            CloudBlobClient client = storageAccount.CreateCloudBlobClient();
            CloudBlobContainer container = client.GetContainerReference(containerName);
            Assert.IsFalse(container.ExistsAsync().Result, "Test container should not exist.");

            new AzureBlobStorage(storageConnectionString, containerName, cryptographer, logger.Object);

            Assert.IsTrue(container.ExistsAsync().Result, "Test container should be created.");
        }

        [TestMethod]
        [TestCategory("DevOnly")]
        public async Task UploadAndDownloadFileTest()
        {
            AzureBlobStorage storage = new AzureBlobStorage(storageConnectionString, containerName, cryptographer, logger.Object);
            string fileName = "TestFile";
            byte[] fileContent = Utils.GenerateByteArray(1000);
            Assert.IsFalse(await storage.FileExists(fileName));
            Assert.IsTrue(await storage.UploadFile(fileName, fileContent));
            Assert.IsTrue(await storage.FileExists(fileName));
            byte[] downloadedContent = await storage.DownloadFile(fileName);
            Assert.IsTrue(downloadedContent.SequenceEqual(fileContent));
        }

        [TestMethod]
        [TestCategory("DevOnly")]
        public async Task UploadInvalidFileNameTest()
        {
            AzureBlobStorage storage = new AzureBlobStorage(storageConnectionString, containerName, cryptographer, logger.Object);
            byte[] fileContent = Utils.GenerateByteArray(1000);
            Assert.IsFalse(await storage.UploadFile(null, fileContent));
            Assert.IsFalse(await storage.UploadFile("", fileContent));
        }

        [TestMethod]
        [TestCategory("DevOnly")]
        public async Task UploadInvalidContentTest()
        {
            AzureBlobStorage storage = new AzureBlobStorage(storageConnectionString, containerName, cryptographer, logger.Object);
            string fileName = "TestFile";
            Assert.IsFalse(await storage.UploadFile(fileName, null));
            Assert.IsFalse(await storage.UploadFile(fileName, new byte[0]));
        }

        [TestMethod]
        [TestCategory("DevOnly")]
        public async Task DownloadNotExistFileTest()
        {
            AzureBlobStorage storage = new AzureBlobStorage(storageConnectionString, containerName, cryptographer, logger.Object);
            string fileName = "TestFile";
            Assert.IsFalse(await storage.FileExists(fileName));
            byte[] downloadedContent = await storage.DownloadFile(fileName);
            Assert.AreEqual(0, downloadedContent.Length);
        }

        [TestMethod]
        [TestCategory("DevOnly")]
        public async Task DownloadInvalidFileNameTest()
        {
            AzureBlobStorage storage = new AzureBlobStorage(storageConnectionString, containerName, cryptographer, logger.Object);
            // null file name
            byte[] downloadedContent = await storage.DownloadFile(null);
            Assert.AreEqual(0, downloadedContent.Length);

            // empty file name
            downloadedContent = await storage.DownloadFile("");
            Assert.AreEqual(0, downloadedContent.Length);
        }

        [TestMethod]
        [TestCategory("DevOnly")]
        public async Task FileExistsTest()
        {
            AzureBlobStorage storage = new AzureBlobStorage(storageConnectionString, containerName, cryptographer, logger.Object);
            string fileName = "TestFile";
            byte[] fileContent = Utils.GenerateByteArray(1000);
            Assert.IsFalse(await storage.FileExists(fileName));
            Assert.IsTrue(await storage.UploadFile(fileName, fileContent));
            Assert.IsTrue(await storage.FileExists(fileName));
        }

        [TestMethod]
        [TestCategory("DevOnly")]
        public async Task FileExistsWithInvalidFileNameTest()
        {
            AzureBlobStorage storage = new AzureBlobStorage(storageConnectionString, containerName, cryptographer, logger.Object);
            // null file name
            Assert.IsFalse(await storage.FileExists(null));

            // empty file name
            Assert.IsFalse(await storage.FileExists(""));
        }

        [TestMethod]
        [TestCategory("DevOnly")]
        public async Task DeleteExistFileTest()
        {
            AzureBlobStorage storage = new AzureBlobStorage(storageConnectionString, containerName, cryptographer, logger.Object);
            string fileName = "TestFile";
            byte[] fileContent = Utils.GenerateByteArray(1000);
            Assert.IsTrue(await storage.UploadFile(fileName, fileContent));
            Assert.IsTrue(await storage.FileExists(fileName));
            Assert.IsTrue(await storage.DeleteFile(fileName));
            Assert.IsFalse(await storage.FileExists(fileName));
        }

        [TestMethod]
        [TestCategory("DevOnly")]
        public async Task DeleteNotExistFileTest()
        {
            AzureBlobStorage storage = new AzureBlobStorage(storageConnectionString, containerName, cryptographer, logger.Object);
            string fileName = "TestFile";
            Assert.IsFalse(await storage.FileExists(fileName));
            Assert.IsTrue(await storage.DeleteFile(fileName));
        }

        [TestMethod]
        [TestCategory("DevOnly")]
        public async Task DeleteInvalidFileNameTest()
        {
            AzureBlobStorage storage = new AzureBlobStorage(storageConnectionString, containerName, cryptographer, logger.Object);
            // null file name
            Assert.IsTrue(await storage.DeleteFile(null));

            // empty file name
            Assert.IsTrue(await storage.DeleteFile(""));
        }

        private string GetTestParameter(string key)
        {
            return TestContext.Properties[key].ToString();
        }
    }
}
