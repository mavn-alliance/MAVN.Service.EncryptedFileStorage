using Lykke.HttpClientGenerator;

namespace MAVN.Service.EncryptedFileStorage.Client
{
    /// <summary>
    /// EncryptedFileStorage API aggregating interface.
    /// </summary>
    public class EncryptedFileStorageClient : IEncryptedFileStorageClient
    {
        /// <summary>Interface to EncryptedFileStorage Api.</summary>
        public IEncryptedFileStorageApi Api { get; private set; }

        /// <summary>C-tor</summary>
        public EncryptedFileStorageClient(IHttpClientGenerator httpClientGenerator)
        {
            Api = httpClientGenerator.Generate<IEncryptedFileStorageApi>();
        }
    }
}
