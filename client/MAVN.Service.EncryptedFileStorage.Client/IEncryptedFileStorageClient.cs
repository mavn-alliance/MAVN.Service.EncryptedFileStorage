using JetBrains.Annotations;

namespace MAVN.Service.EncryptedFileStorage.Client
{
    /// <summary>
    /// EncryptedFileStorage client interface.
    /// </summary>
    [PublicAPI]
    public interface IEncryptedFileStorageClient
    {
        /// <summary>Application Api interface</summary>
        IEncryptedFileStorageApi Api { get; }
    }
}
