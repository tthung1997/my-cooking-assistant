using Microsoft.Azure.KeyVault;
using Microsoft.Azure.KeyVault.Models;
using Microsoft.Azure.Services.AppAuthentication;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Common
{
    public interface ICredentialAccessor
    {
        Task<string> GetSecret(string key);
    }

    public class KeyVaultAccessor : ICredentialAccessor
    {
        private readonly KeyVaultClient keyVaultClient;
        private readonly string keyVaultUrl;

        private readonly ILogger logger;

        public KeyVaultAccessor(string keyVaultUrl,
            ILogger logger,
            string azureServiceAuthConnectionString = "")
        {
            var azureServiceTokenProvider = 
                new AzureServiceTokenProvider(azureServiceAuthConnectionString);
            keyVaultClient = new KeyVaultClient(
                new KeyVaultClient.AuthenticationCallback(
                    azureServiceTokenProvider.KeyVaultTokenCallback));
            this.keyVaultUrl = keyVaultUrl;
            this.logger = logger;
        }

        public async Task<string> GetSecret(string key)
        {
            string secretValue = null;
            try
            {
                await logger.LogInfo($"Retrieving secret {key} from Key Vault {keyVaultUrl}");
                var secretInKeyVault = await keyVaultClient.GetSecretAsync(keyVaultUrl, key);
                await logger.LogInfo($"Secret {key} loaded.");
                secretValue = secretInKeyVault.Value;
            }
            catch (KeyVaultErrorException keyVaultException)
            {
                await logger.LogError(keyVaultException);
            }
            return secretValue;
        }
    }
}
