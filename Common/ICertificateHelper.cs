using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;

namespace Common
{
    public interface ICertificateHelper
    {
        Task<X509Certificate2> GetCertificateByType(X509FindType type, string value);
        Task<X509Certificate2> GetCertificateByType(StoreName storeName, StoreLocation storeLocation, X509FindType type, string value);
    }

    public class CertificateHelper : ICertificateHelper
    {
        private readonly ILogger logger;
        private readonly string loggingSource;

        public CertificateHelper(ILogger logger, string loggingSource = "CertificateHelper")
        {
            this.logger = logger;
            this.loggingSource = loggingSource;
        }

        public async Task<X509Certificate2> GetCertificateByType(X509FindType type, string value)
        {
            var certificate = await GetCertificateByType(StoreName.My, StoreLocation.CurrentUser, type, value);
            if (certificate == null)
            {
                certificate = await GetCertificateByType(StoreName.My, StoreLocation.LocalMachine, type, value);
            }
            return certificate;
        }

        public async Task<X509Certificate2> GetCertificateByType(StoreName storeName, StoreLocation storeLocation, X509FindType type, string value)
        {
            X509Certificate2 certificate = null;
            using (X509Store store = new X509Store(storeName, storeLocation))
            {
                store.Open(OpenFlags.ReadOnly);
                var foundCertificate2Collection = store.Certificates.Find(type, value, true).Cast<X509Certificate2>();

                if (foundCertificate2Collection.Count() > 0)
                {
                    certificate = foundCertificate2Collection.FirstOrDefault();
                }
                else
                {
                    await logger.LogError(loggingSource, new KeyNotFoundException($"Certificate cannot be found. {type}: {value}"));
                }

                store.Close();
            }
            return certificate;
        }
    }
}