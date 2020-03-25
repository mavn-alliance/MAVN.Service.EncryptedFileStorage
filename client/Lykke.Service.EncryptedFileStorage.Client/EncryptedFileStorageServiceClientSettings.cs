using Lykke.SettingsReader.Attributes;

namespace Lykke.Service.EncryptedFileStorage.Client 
{
    /// <summary>
    /// EncryptedFileStorage client settings.
    /// </summary>
    public class EncryptedFileStorageServiceClientSettings 
    {
        /// <summary>Service url.</summary>
        [HttpCheck("api/isalive")]
        public string ServiceUrl {get; set;}

        /// <summary>Api key.</summary>
        [Optional]
        public string ApiKey { get; set; }
    }
}
