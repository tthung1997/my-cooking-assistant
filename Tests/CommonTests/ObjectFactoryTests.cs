using Common;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Text;

namespace CommonTests
{
    [TestClass]
    public class ObjectFactoryTests
    {
        [TestInitialize]
        public void Initialize()
        {
            ObjectFactory.Reset();
        }

        [TestCleanup]
        public void Cleanup()
        {

        }

        [TestMethod]
        [TestCategory("Bvt")]
        public void SkipInitializationTest()
        {
            ILogger logger = new ConsoleLogger();
            try
            {
                ObjectFactory.RegisterInstance<ILogger>(logger);
            }
            catch (InvalidOperationException ioe)
            {
                Assert.AreEqual("Please execute Initialize first", ioe.Message);
            }

            try
            {
                ObjectFactory.GetInstance<ILogger>();
            }
            catch (InvalidOperationException ioe)
            {
                Assert.AreEqual("Please execute Initialize first", ioe.Message);
            }

            try
            {
                ObjectFactory.RegisterInstance<ILogger>(Guid.Empty, logger);
            }
            catch (InvalidOperationException ioe)
            {
                Assert.AreEqual("Please execute Initialize with the specified domain first", ioe.Message);
            }

            try
            {
                ObjectFactory.GetInstance<ILogger>(Guid.Empty);
            }
            catch (InvalidOperationException ioe)
            {
                Assert.AreEqual("Please execute Initialize with the specified domain first", ioe.Message);
            }
        }

        [TestMethod]
        [TestCategory("Bvt")]
        public void FailToGetInstanceTest()
        {
            ObjectFactory.Initialize(Guid.Empty);
            try
            {
                ObjectFactory.GetInstance<ILogger>();
            }
            catch (KeyNotFoundException knfe)
            {
                Assert.AreEqual($"The given key '{typeof(ILogger)}' was not present in the dictionary.", knfe.Message);
            }

            try
            {
                ObjectFactory.GetInstance<ILogger>(Guid.Empty);
            }
            catch (KeyNotFoundException knfe)
            {
                Assert.AreEqual($"The given key '{typeof(ILogger)}' was not present in the dictionary.", knfe.Message);
            }
        }

        [TestMethod]
        [TestCategory("Bvt")]
        public void SuccessfullyGetInstanceTest()
        {
            ObjectFactory.Initialize(Guid.Empty);
            ILogger logger = new ConsoleLogger();
            ObjectFactory.RegisterInstance<ILogger>(logger);
            Assert.AreEqual(logger, ObjectFactory.GetInstance<ILogger>());

            Guid newGuid = Guid.NewGuid();
            ObjectFactory.Initialize(newGuid);
            ILogger fileLogger = new FileLogger("log.txt");
            ObjectFactory.RegisterInstance<ILogger>(newGuid, fileLogger);
            Assert.AreEqual(fileLogger, ObjectFactory.GetInstance<ILogger>(newGuid));
        }

        [TestMethod]
        [TestCategory("Bvt")]
        public void InitializeWithoutForceTest()
        {
            ObjectFactory.Initialize(Guid.Empty);
            ILogger logger = new ConsoleLogger();
            ObjectFactory.RegisterInstance<ILogger>(logger);

            ObjectFactory.Initialize(Guid.Empty);
            Assert.AreEqual(logger, ObjectFactory.GetInstance<ILogger>());
        }

        [TestMethod]
        [TestCategory("Bvt")]
        public void InitializeWithForceTest()
        {
            ObjectFactory.Initialize(Guid.Empty);
            ILogger logger = new ConsoleLogger();
            ObjectFactory.RegisterInstance<ILogger>(logger);

            ObjectFactory.Initialize(Guid.Empty, true);
            try
            {
                ObjectFactory.GetInstance<ILogger>();
            }
            catch (KeyNotFoundException knfe)
            {
                Assert.AreEqual($"The given key '{typeof(ILogger)}' was not present in the dictionary.", knfe.Message);
            }
        }

        [TestMethod]
        [TestCategory("Bvt")]
        public void GetLatestInstanceTest()
        {
            ObjectFactory.Initialize(Guid.Empty);
            ILogger consoleLogger = new ConsoleLogger();
            ObjectFactory.RegisterInstance<ILogger>(consoleLogger);
            ILogger fileLogger = new FileLogger("log.txt");
            ObjectFactory.RegisterInstance<ILogger>(fileLogger);
            Assert.AreEqual(fileLogger, ObjectFactory.GetInstance<ILogger>());
        }

        [TestMethod]
        [TestCategory("Bvt")]
        public void ResetTest()
        {
            ObjectFactory.Initialize(Guid.Empty);
            ILogger logger = new ConsoleLogger();
            ObjectFactory.RegisterInstance<ILogger>(logger);

            ObjectFactory.Reset();

            try
            {
                ObjectFactory.GetInstance<ILogger>();
            }
            catch (InvalidOperationException ioe)
            {
                Assert.AreEqual("Please execute Initialize first", ioe.Message);
            }

            try
            {
                ObjectFactory.GetInstance<ILogger>(Guid.Empty);
            }
            catch (InvalidOperationException ioe)
            {
                Assert.AreEqual("Please execute Initialize with the specified domain first", ioe.Message);
            }
        }
    }
}
