using Common;
using Microsoft.VisualStudio.TestPlatform.Common.Exceptions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace CommonTests
{
    [TestClass]
    public class ConsoleLoggerTests
    {
        private ILogger logger;
        private const string loggingSource = "Test";

        [TestInitialize]
        public void Initialize()
        {
            logger = new ConsoleLogger();
        }

        [TestMethod]
        [TestCategory("Bvt")]
        public async Task LogInfoTest()
        {
            using (var sw = new StringWriter())
            {
                var info = "Test Info";
                Console.SetOut(sw);
                await logger.LogInfo(loggingSource, info);
                var output = sw.ToString();
                Assert.IsTrue(output.Contains("[INFO]"));
                Assert.IsTrue(output.Contains("[Test]"));
                Assert.IsTrue(output.Contains(info));
                sw.Close();
            }
        }

        [TestMethod]
        [TestCategory("Bvt")]
        public async Task LogErrorStringTest()
        {
            using (var sw = new StringWriter())
            {
                var error = "Test Error";
                Console.SetOut(sw);
                await logger.LogError(loggingSource, error);
                var output = sw.ToString();
                Assert.IsTrue(output.Contains("[ERROR]"));
                Assert.IsTrue(output.Contains("[Test]"));
                Assert.IsTrue(output.Contains(error));
                sw.Close();
            }
        }

        [TestMethod]
        [TestCategory("Bvt")]
        public async Task LogExceptionTest()
        {
            using (var sw = new StringWriter())
            {
                var errorMsg = "TestError";
                var error = new Exception(errorMsg);
                Console.SetOut(sw);
                await logger.LogError(loggingSource, error);
                var output = sw.ToString();
                Assert.IsTrue(output.Contains("[ERROR]"));
                Assert.IsTrue(output.Contains("[Test]"));
                Assert.IsTrue(output.Contains(errorMsg));
                Assert.IsTrue(output.Contains("Trace"));
                sw.Close();
            }
        }

        [TestMethod]
        [TestCategory("Bvt")]
        public async Task LogWarningTest()
        {
            using (var sw = new StringWriter())
            {
                var warning = "Test Warning";
                Console.SetOut(sw);
                await logger.LogWarning(loggingSource, warning);
                var output = sw.ToString();
                Assert.IsTrue(output.Contains("[WARNING]"));
                Assert.IsTrue(output.Contains("[Test]"));
                Assert.IsTrue(output.Contains(warning));
                sw.Close();
            }
        }
    }
}
