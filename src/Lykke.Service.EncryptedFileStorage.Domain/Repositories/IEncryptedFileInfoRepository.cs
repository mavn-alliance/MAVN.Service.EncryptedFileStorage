using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Lykke.Service.EncryptedFileStorage.Domain.Models;

namespace Lykke.Service.EncryptedFileStorage.Domain.Repositories
{
    public interface IEncryptedFileInfoRepository
    {
        Task<Guid> CreateFileInfoAsync(EncryptedFile data);
        Task<bool> DeleteFileInfoAsync(Guid fileId);
        Task<bool> UpdateFileInfoAsync(EncryptedFile data);
        Task<EncryptedFile> GetFileInfoAsync(Guid fileId);
        Task<IEnumerable<EncryptedFile>> GetPaginatedFilesInfoAsync(int skip, int take);
        Task<int> GetTotalAsync();
        Task<IEnumerable<EncryptedFile>> GetPaginatedFilesInfoByOriginAsync(int skip, int take, string origin);
        Task<bool> FileInfoByOriginAndFileNameExistsAsync(string origin, string fileName);
        Task<int> GetTotalByOriginAsync(string origin);
    }
}
