using Common;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace CommonTests
{
    [TestClass]
    public class CertificateHelperTests
    {
        private const string certificateThumbprint = "4066737bb86c29e53a18d3cef83df5d18dcfc323"; // mca-test.ht
        private const string certificateSubject = "CN=mca-test.ht";
        private const string certificateIssuer = "CN=mca-test.ht";

        private ICertificateHelper certHelper;

        [TestInitialize]
        public void Initialize()
        {
            certHelper = new CertificateHelper();
        }

        [TestCleanup]
        public void Cleanup()
        {
        }

        [TestMethod]
        [TestCategory("DevOnly")]
        public async Task GetCertificateByTypeFromAllStoresTest()
        {
            X509Certificate2 foundCert = await certHelper.GetCertificateByType(X509FindType.FindByThumbprint, certificateThumbprint);
            Assert.AreEqual(certificateThumbprint, foundCert.Thumbprint, true);
            Assert.AreEqual(certificateSubject, foundCert.Subject, true);
            Assert.AreEqual(certificateIssuer, foundCert.Issuer, true);
        }

        [TestMethod]
        [TestCategory("DevOnly")]
        public async Task GetCertificateByTypeFromMyCurrentUserTest()
        {
            X509Certificate2 foundCert = await certHelper.GetCertificateByType(StoreName.My, StoreLocation.CurrentUser, X509FindType.FindByThumbprint, certificateThumbprint);
            Assert.AreEqual(certificateThumbprint, foundCert.Thumbprint, true);
            Assert.AreEqual(certificateSubject, foundCert.Subject, true);
            Assert.AreEqual(certificateIssuer, foundCert.Issuer, true);
        }

        [TestMethod]
        [TestCategory("DevOnly")]
        public async Task GetCertificateByTypeFromMyLocalMachineTest()
        {
            X509Certificate2 foundCert = await certHelper.GetCertificateByType(StoreName.My, StoreLocation.LocalMachine, X509FindType.FindByThumbprint, certificateThumbprint);
            Assert.AreEqual(certificateThumbprint, foundCert.Thumbprint, true);
            Assert.AreEqual(certificateSubject, foundCert.Subject, true);
            Assert.AreEqual(certificateIssuer, foundCert.Issuer, true);
        }

        [TestMethod]
        [TestCategory("DevOnly")]
        public async Task GetCertificateBySubjectIssuerTest()
        {
            X509Certificate2 foundCert = await certHelper.GetCertificate(certificateSubject, new string[] { certificateIssuer });
            Assert.AreEqual(certificateThumbprint, foundCert.Thumbprint, true);
            Assert.AreEqual(certificateSubject, foundCert.Subject, true);
            Assert.AreEqual(certificateIssuer, foundCert.Issuer, true);
        }

        [TestMethod]
        [TestCategory("Bvt")]
        public async Task FailToGetCertificateBySubjectIssuerTest()
        {
            string testSubject = "CN=cert.does.not.exist";
            string testIssuer = "CN=cert.does.not.exist";
            try
            {
                X509Certificate2 foundCert = await certHelper.GetCertificate(testSubject, new string[] { testIssuer });
                Assert.Fail("Should throw KeyNotFoundException");
            }
            catch (KeyNotFoundException knfe)
            {
                Assert.IsTrue(knfe.Message.Contains("Certificate cannot be found."));
            } 
        }

        [TestMethod]
        [TestCategory("Bvt")]
        public async Task FailToGetCertificateByTypeFromAllStoresTest()
        {
            string testSubject = "CN=cert.does.not.exist";
            try
            {
                X509Certificate2 foundCert = await certHelper.GetCertificateByType(X509FindType.FindBySubjectDistinguishedName, testSubject);
                Assert.Fail("Should throw KeyNotFoundException");
            }
            catch (KeyNotFoundException knfe)
            {
                Assert.IsTrue(knfe.Message.Contains("Certificate cannot be found."));
            }
        }

        [TestMethod]
        [TestCategory("Bvt")]
        public async Task FailToGetCertificateByTypeFromMyCurrentUserTest()
        {
            string testSubject = "CN=cert.does.not.exist";
            try
            {
                X509Certificate2 foundCert = await certHelper.GetCertificateByType(StoreName.My, StoreLocation.CurrentUser, X509FindType.FindBySubjectDistinguishedName, testSubject);
                Assert.Fail("Should throw KeyNotFoundException");
            }
            catch (KeyNotFoundException knfe)
            {
                Assert.IsTrue(knfe.Message.Contains("Certificate cannot be found."));
            }
        }

        [TestMethod]
        [TestCategory("Bvt")]
        public async Task FailToGetCertificateByTypeFromMyLocalMachineTest()
        {
            string testSubject = "CN=cert.does.not.exist";
            try
            {
                X509Certificate2 foundCert = await certHelper.GetCertificateByType(StoreName.My, StoreLocation.LocalMachine, X509FindType.FindBySubjectDistinguishedName, testSubject);
                Assert.Fail("Should throw KeyNotFoundException");
            }
            catch (KeyNotFoundException knfe)
            {
                Assert.IsTrue(knfe.Message.Contains("Certificate cannot be found."));
            }
        }
    }
}
