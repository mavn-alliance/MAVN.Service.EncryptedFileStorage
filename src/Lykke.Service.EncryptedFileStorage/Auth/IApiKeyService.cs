namespace Lykke.Service.EncryptedFileStorage.Auth
{
    public interface IApiKeyService
    {
        bool ValidateKey(string apiKey);
        string GetKeyName(string apiKey);
    }
}
