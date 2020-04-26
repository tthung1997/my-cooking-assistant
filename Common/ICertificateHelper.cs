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
        Task<X509Certificate2> GetCertificate(string subject, string[] trustedIssuers);
    }

    public class CertificateHelper : ICertificateHelper
    {
        public Task<X509Certificate2> GetCertificate(string subject, string[] trustedIssuers)
        {
            // get certs by subject
            IEnumerable<X509Certificate2> certificates =
                (GetCertificatesByType(StoreName.My, StoreLocation.CurrentUser,
                    X509FindType.FindBySubjectDistinguishedName, subject) ?? Enumerable.Empty<X509Certificate2>())
                .Concat(GetCertificatesByType(StoreName.My, StoreLocation.LocalMachine,
                    X509FindType.FindBySubjectDistinguishedName, subject) ?? Enumerable.Empty<X509Certificate2>());
            // filter by issuers
            certificates = certificates.Where(c => trustedIssuers.Contains(c.Issuer));
            X509Certificate2 foundCertificate = certificates.DefaultIfEmpty(null).FirstOrDefault();
            if (foundCertificate == null)
            {
                throw new KeyNotFoundException($"Certificate cannot be found. " +
                    $"Subject: {subject}, Trusted issuers: {string.Join(", ", trustedIssuers)}");
            }
            return Task.FromResult(foundCertificate);
        }

        public async Task<X509Certificate2> GetCertificateByType(X509FindType type, string value)
        {
            X509Certificate2 foundCertificate;
            try
            {
                foundCertificate = await GetCertificateByType(StoreName.My, StoreLocation.CurrentUser, type, value);
            }
            catch (KeyNotFoundException knfe1)
            {
                if (knfe1.Message.Contains($"Certificate cannot be found"))
                {
                    try
                    {
                        foundCertificate = await GetCertificateByType(StoreName.My, StoreLocation.LocalMachine, type, value);
                    }
                    catch (KeyNotFoundException knfe2)
                    {
                        if (knfe2.Message.Contains($"Certificate cannot be found"))
                        {
                            throw new KeyNotFoundException($"Certificate cannot be found. {type}: {value}");
                        }
                        else
                        {
                            throw knfe2;
                        }
                    }
                }
                else
                {
                    throw knfe1;
                }
            }
            return foundCertificate;
        }

        public Task<X509Certificate2> GetCertificateByType(StoreName storeName, StoreLocation storeLocation, X509FindType type, string value)
        {
            X509Certificate2 certificate;
            IEnumerable<X509Certificate2> foundCertificate2Collection = GetCertificatesByType(storeName, storeLocation, type, value);
            if (foundCertificate2Collection.Count() > 0)
            {
                certificate = foundCertificate2Collection.FirstOrDefault();
            }
            else
            {
                throw new KeyNotFoundException($"Certificate cannot be found. Store Name: {storeName}, Store Location: {storeLocation}, {type}: {value}");
            }
            return Task.FromResult(certificate);
        }

        private IEnumerable<X509Certificate2> GetCertificatesByType(StoreName storeName, StoreLocation storeLocation, X509FindType type, string value)
        {
            IEnumerable<X509Certificate2> foundCertificate2Collection = new List<X509Certificate2>();
            using (X509Store store = new X509Store(storeName, storeLocation))
            {
                store.Open(OpenFlags.ReadOnly);
                foundCertificate2Collection = store.Certificates.Find(type, value, false).Cast<X509Certificate2>();
                store.Close();
            }
            return foundCertificate2Collection;
        }
    }
}