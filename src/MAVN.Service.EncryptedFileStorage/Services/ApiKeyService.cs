using System;
using System.Collections.Generic;
using System.Linq;
using MAVN.Service.EncryptedFileStorage.Auth;

namespace MAVN.Service.EncryptedFileStorage.Services
{
    /// <summary>
    /// Validator for auth api key
    /// </summary>
    public class ApiKeyService : IApiKeyService
    {
        private readonly Dictionary<string, string> _apiKeys;

        public ApiKeyService(string apiKeysStr)
        {
            if (!string.IsNullOrWhiteSpace(apiKeysStr))
            {
                var apiKeyParts = apiKeysStr.Split('|');
                if (apiKeyParts.Length % 2 != 0)
                    throw new InvalidOperationException("Api keys env var has inconsistent value");

                _apiKeys = new Dictionary<string, string>(apiKeyParts.Length / 2);
                for (int i = 0; i < apiKeyParts.Length; i += 2)
                {
                    _apiKeys.Add(apiKeyParts[i + 1], apiKeyParts[i]);
                }
            }
        }

        public bool ValidateKey(string apiKey)
        {
            return _apiKeys.ContainsKey(apiKey);
        }

        public string GetKeyName(string apiKey)
        {
            _apiKeys.TryGetValue(apiKey, out var apiKeyName);

            return apiKeyName;
        }
    }
}
