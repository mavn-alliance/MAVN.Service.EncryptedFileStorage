namespace MAVN.Service.EncryptedFileStorage.Client.Models.Requests
{
    /// <summary>
    /// Represent file that gonna be encrypted and stored
    /// </summary>
    public class CreateFileInfoRequest
    {
        /// <summary>
        /// File origin (service name) that this file came from
        /// </summary>
        public string Origin { get; set; }

        /// <summary>
        /// File name (unique to specified origin)
        /// </summary>
        public string FileName { get; set; }
    }
}
