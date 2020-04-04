using System.ComponentModel.DataAnnotations;

namespace MAVN.Service.EncryptedFileStorage.Models
{
    /// <summary>
    /// This is request model for setting encryption key used for encrypting the data
    /// </summary>
    public class SetKeyRequest
    {
        /// <summary>
        /// 256 bit encryption key in base64-string format
        /// </summary>
        [Required]
        public string Key { get; set; }
    }
}
