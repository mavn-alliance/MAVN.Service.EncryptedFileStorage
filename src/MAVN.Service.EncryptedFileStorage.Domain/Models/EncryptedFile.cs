using System;

namespace MAVN.Service.EncryptedFileStorage.Domain.Models
{
    /// <summary>
    /// Encrypted file model
    /// </summary>
    public class EncryptedFile
    {
        /// <summary>
        /// Id
        /// </summary>
        public Guid FileId { get; set; }

        /// <summary>
        /// File origin (service name) that this file came from
        /// </summary>
        public string Origin { get; set; }

        /// <summary>
        /// File name (unique to specified origin)
        /// </summary>
        public string FileName { get; set; }

        /// <summary>
        /// Trimmed version of the FileName according to the allowed characters in Azure Blob names + GUID
        /// </summary>
        public string BlobName { get; set; }

        /// <summary>
        /// Length of the unencrypted file
        /// </summary>
        public long? Length { get; set; }

        /// <summary>
        /// Date of upload
        /// </summary>
        public DateTime? FileDate { get; set; }

        /// <summary>
        /// File content
        /// Stored in Azure Storage as a blob
        /// </summary>
        public string Content { get; set; }

        /// <summary>
        /// Flag to mark when file content upload is completed
        /// </summary>
        public bool IsUploadCompleted { get; set; }
    }
}
