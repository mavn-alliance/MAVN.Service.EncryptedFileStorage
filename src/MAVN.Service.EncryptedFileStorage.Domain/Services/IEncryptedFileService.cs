using System;
using System.IO;
using System.Threading.Tasks;
using MAVN.Service.EncryptedFileStorage.Domain.Models;
using Microsoft.AspNetCore.Http;

namespace MAVN.Service.EncryptedFileStorage.Domain.Services
{
    public interface IEncryptedFileService
    {
        Task<Guid> CreateFileInfoAsync(EncryptedFile data);
        Task DeleteFileAsync(Guid fileId);
        Task<EncryptedFile> GetFileMetadataAsync(Guid fileId);
        Task<PaginatedEncryptedFiles> GetPaginatedFilesMetadataAsync(int currentPage, int pageSize);
        Task<PaginatedEncryptedFiles> GetPaginatedFilesMetadataByOriginAsync(int currentPage, int pageSize, string origin);
        Task<EncryptedFile> EncryptAndStoreFileContentAsync(Guid fileId, IFormFile formFile);
        Task<Stream> DecryptAndDownloadFileContentAsync(Guid fileId);
        string PrepareContainerName(string origin);
        string PrepareBlobName(string fileName, Guid fileId);
    }
}
