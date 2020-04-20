using Common;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Threading.Tasks;

namespace CommonTests
{
    [TestClass]
    public class FileLoggerTests
    {
        private ILogger logger;
        private const string loggingSource = "Test";
        private const string filePath = "log.txt";

        [TestInitialize]
        public void Initialize()
        {
            logger = new FileLogger(filePath);
        }

        [TestMethod]
        [TestCategory("Bvt")]
        public async Task LogInfoTest()
        {
            var info = "Test Info";
            await logger.LogInfo(loggingSource, info);
            var output = File.ReadAllText(filePath);
            Assert.IsTrue(output.Contains("[INFO]"));
            Assert.IsTrue(output.Contains("[Test]"));
            Assert.IsTrue(output.Contains(info));
        }

        [TestMethod]
        [TestCategory("Bvt")]
        public async Task LogErrorStringTest()
        {
            var error = "Test Error";
            await logger.LogError(loggingSource, error);
            var output = File.ReadAllText(filePath);
            Assert.IsTrue(output.Contains("[ERROR]"));
            Assert.IsTrue(output.Contains("[Test]"));
            Assert.IsTrue(output.Contains(error));
        }

        [TestMethod]
        [TestCategory("Bvt")]
        public async Task LogExceptionTest()
        {
            var errorMsg = "TestError";
            var error = new Exception(errorMsg);
            await logger.LogError(loggingSource, error);
            var output = File.ReadAllText(filePath);
            Assert.IsTrue(output.Contains("[ERROR]"));
            Assert.IsTrue(output.Contains("[Test]"));
            Assert.IsTrue(output.Contains(errorMsg));
            Assert.IsTrue(output.Contains("Trace"));
        }

        [TestMethod]
        [TestCategory("Bvt")]
        public async Task LogWarningTest()
        {
            var warning = "Test Warning";
            await logger.LogWarning(loggingSource, warning);
            var output = File.ReadAllText(filePath);
            Assert.IsTrue(output.Contains("[WARNING]"));
            Assert.IsTrue(output.Contains("[Test]"));
            Assert.IsTrue(output.Contains(warning));
        }
    }
}
