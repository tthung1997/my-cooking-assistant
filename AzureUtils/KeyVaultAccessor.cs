using Common;
using Microsoft.Azure.KeyVault;
using Microsoft.Azure.KeyVault.Models;
using Microsoft.Azure.Services.AppAuthentication;
using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace AzureUtils
{
    public class KeyVaultAccessor : ICredentialAccessor
    {
        private const string keyVaultUrlRegexPattern = @"^https://[a-zA-Z0-9\-]+.vault.azure.net/?$";

        private readonly KeyVaultClient keyVaultClient;
        private readonly string keyVaultUrl;

        private readonly string loggingSource;
        private readonly ILogger logger;

        public KeyVaultAccessor(string keyVaultUrl,
            string loggingSource,
            ILogger logger,
            string azureServiceAuthConnectionString = "")
        {
            var azureServiceTokenProvider =
                new AzureServiceTokenProvider(azureServiceAuthConnectionString);
            keyVaultClient = new KeyVaultClient(
                new KeyVaultClient.AuthenticationCallback(
                    azureServiceTokenProvider.KeyVaultTokenCallback));

            var regex = new Regex(keyVaultUrlRegexPattern);
            if (!regex.IsMatch(keyVaultUrl))
            {
                throw new ArgumentException(nameof(keyVaultUrl));
            }
            this.keyVaultUrl = keyVaultUrl;
            this.logger = logger;
            this.loggingSource = loggingSource;
        }

        public async Task<string> GetSecret(string key)
        {
            string secretValue = null;
            try
            {
                await logger.LogInfo(loggingSource, $"Retrieving secret {key} from Key Vault {keyVaultUrl}");
                var secretInKeyVault = await keyVaultClient.GetSecretAsync(keyVaultUrl, key);
                await logger.LogInfo(loggingSource, $"Secret {key} loaded.");
                secretValue = secretInKeyVault.Value;
            }
            catch (KeyVaultErrorException keyVaultException)
            {
                await logger.LogError(loggingSource, keyVaultException);
            }
            return secretValue;
        }
    }
}
