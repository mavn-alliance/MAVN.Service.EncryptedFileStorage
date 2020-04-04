using System.Collections.Generic;

namespace MAVN.Service.EncryptedFileStorage.Domain.Models
{
    public class PaginatedEncryptedFiles
    {
        public int CurrentPage { get; set; }
        public int PageSize { get; set; }
        public int TotalCount { get; set; }
        public IEnumerable<EncryptedFile> Files { get; set; }
    }
}
