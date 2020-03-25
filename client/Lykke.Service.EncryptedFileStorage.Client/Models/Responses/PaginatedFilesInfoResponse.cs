using System.Collections.Generic;
using JetBrains.Annotations;

namespace Lykke.Service.EncryptedFileStorage.Client.Models.Responses
{
    /// <summary>
    /// Gives the list of files info on the desired page and information about the page
    /// </summary>
    [PublicAPI]
    public class PaginatedFilesInfoResponse
    {
        /// <summary>
        /// Current page in pagination result
        /// </summary>
        public int CurrentPage { get; set; }

        /// <summary>
        /// Page size
        /// </summary>
        public int PageSize { get; set; }

        /// <summary>
        /// Total count of records
        /// </summary>
        public int TotalCount { get; set; }

        /// <summary>
        /// List of Customers for the given page
        /// </summary>
        public IEnumerable<FileInfoResponse> Files { get; set; }
    }
}
