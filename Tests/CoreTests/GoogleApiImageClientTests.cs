using Common;
using Core;
using Google.Apis.Customsearch.v1.Data;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CoreTests
{
    [TestClass]
    public class GoogleApiImageClientTest
    {
        private readonly string testQuery = "Bill Gates";
        private string apiKey;
        private string searchEngineId;
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
            apiKey = GetTestParameter("googleApiKey");
            searchEngineId = GetTestParameter("googleSearchEngineId");
            logger = new Mock<ILogger>();
        }

        [TestCleanup]
        public void Cleanup()
        {

        }

        [TestMethod]
        [TestCategory("Bvt")]
        public void NullLoggerTest()
        {
            try
            {
                IImageClient client = new GoogleApiImageClient(apiKey, searchEngineId, null);
                Assert.Fail("Should fail because logger is null");
            }
            catch (ArgumentNullException ane)
            {
                Assert.AreEqual("Value cannot be null. (Parameter 'logger')", ane.Message);
            }
        }

        [TestMethod]
        [TestCategory("Bvt")]
        public void InvalidApiKeyTest()
        {
            // null api key
            try
            {
                IImageClient client = new GoogleApiImageClient(null, searchEngineId, logger.Object);
                Assert.Fail("Should fail because apiKey is null");
            }
            catch (ArgumentNullException ane)
            {
                Assert.AreEqual("Value cannot be null. (Parameter 'apiKey')", ane.Message);
            }

            // empty api key
            try
            {
                IImageClient client = new GoogleApiImageClient("", searchEngineId, logger.Object);
                Assert.Fail("Should fail because apiKey is empty");
            }
            catch (ArgumentNullException ane)
            {
                Assert.AreEqual("Value cannot be null. (Parameter 'apiKey')", ane.Message);
            }

            // whitespace api key
            try
            {
                IImageClient client = new GoogleApiImageClient(" ", searchEngineId, logger.Object);
                Assert.Fail("Should fail because apiKey is invalid");
            }
            catch (ArgumentNullException ane)
            {
                Assert.AreEqual("Value cannot be null. (Parameter 'apiKey')", ane.Message);
            }
        }

        [TestMethod]
        [TestCategory("Bvt")]
        public void InvalidSearchEngineIdTest()
        {
            // null api key
            try
            {
                IImageClient client = new GoogleApiImageClient(apiKey, null, logger.Object);
                Assert.Fail("Should fail because searchEngineId is null");
            }
            catch (ArgumentNullException ane)
            {
                Assert.AreEqual("Value cannot be null. (Parameter 'searchEngineId')", ane.Message);
            }

            // empty api key
            try
            {
                IImageClient client = new GoogleApiImageClient(apiKey, "", logger.Object);
                Assert.Fail("Should fail because searchEngineId is empty");
            }
            catch (ArgumentNullException ane)
            {
                Assert.AreEqual("Value cannot be null. (Parameter 'searchEngineId')", ane.Message);
            }

            // whitespace api key
            try
            {
                IImageClient client = new GoogleApiImageClient(apiKey, " ", logger.Object);
                Assert.Fail("Should fail because searchEngineId is invalid");
            }
            catch (ArgumentNullException ane)
            {
                Assert.AreEqual("Value cannot be null. (Parameter 'searchEngineId')", ane.Message);
            }
        }

        [TestMethod]
        [TestCategory("Bvt")]
        public async Task InvalidQueryTest()
        {
            // null query
            try
            {
                IImageClient client = new GoogleApiImageClient(apiKey, searchEngineId, logger.Object);
                IEnumerable<DownloadedImage> images = await client.GetImages(null, 10);
                Assert.Fail("Should fail because query is null");
            }
            catch (ArgumentNullException ane)
            {
                Assert.AreEqual("Value cannot be null. (Parameter 'query')", ane.Message);
            }

            // empty query
            try
            {
                IImageClient client = new GoogleApiImageClient(apiKey, searchEngineId, logger.Object);
                IEnumerable<DownloadedImage> images = await client.GetImages("", 10);
                Assert.Fail("Should fail because query is empty");
            }
            catch (ArgumentNullException ane)
            {
                Assert.AreEqual("Value cannot be null. (Parameter 'query')", ane.Message);
            }

            // whitespace query
            try
            {
                IImageClient client = new GoogleApiImageClient(apiKey, searchEngineId, logger.Object);
                IEnumerable<DownloadedImage> images = await client.GetImages(" ", 10);
                Assert.Fail("Should fail because query is invalid");
            }
            catch (ArgumentNullException ane)
            {
                Assert.AreEqual("Value cannot be null. (Parameter 'query')", ane.Message);
            }
        }

        [TestMethod]
        [TestCategory("Bvt")]
        public async Task NegativeCountTest()
        {
            // negative
            try
            {
                IImageClient client = new GoogleApiImageClient(apiKey, searchEngineId, logger.Object);
                IEnumerable<DownloadedImage> images = await client.GetImages(testQuery, -1);
                Assert.Fail("Should fail because count is negative");
            }
            catch (ArgumentException ae)
            {
                Assert.AreEqual("count", ae.Message);
            }
        }

        [TestMethod]
        [TestCategory("Bvt")]
        public async Task ZeroCountTest()
        {
            IImageClient client = new GoogleApiImageClient(apiKey, searchEngineId, logger.Object);
            IEnumerable<DownloadedImage> images = await client.GetImages(testQuery, 0);
            Assert.AreEqual(0, images.Count());
        }

        [TestMethod]
        [TestCategory("Bvt")]
        public async Task SuccessfullGetImagesTest()
        {
            IImageClient client = new GoogleApiImageClient(apiKey, searchEngineId, logger.Object);
            IEnumerable<DownloadedImage> images = await client.GetImages(testQuery, 10);
            Assert.AreEqual(10, images.Count());
        }

        private string GetTestParameter(string key)
        {
            return TestContext.Properties[key].ToString();
        }
    }
}
