using AzureUtils;
using Common;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Runtime.CompilerServices;

namespace AzureUtilsTests
{
    [TestClass]
    public class AzureBlobStorageTests
    {
        private string storageConnectionString;
        private ILogger logger;

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
            logger = new ConsoleLogger();
            storageConnectionString = GetTestParameter("storageConnectionString");
        }

        [TestMethod]
        public void UnitTest1()
        {

        }

        private string GetTestParameter(string key)
        {
            return TestContext.Properties[key].ToString();
        }
    }
}
