using System;
using System.Linq;
using System.Runtime.CompilerServices;
using System.IO;
using System.Threading.Tasks;
using Common.Log;
using Lykke.Common.Log;
using MAVN.Service.EncryptedFileStorage.Domain.Exceptions;
using MAVN.Service.EncryptedFileStorage.Domain.Models;
using MAVN.Service.EncryptedFileStorage.Domain.Repositories;
using MAVN.Service.EncryptedFileStorage.Domain.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.KeyVault;
using Microsoft.Azure.Storage.Blob;

[assembly: InternalsVisibleTo("MAVN.Service.EncryptedFileStorage.Tests")]

namespace MAVN.Service.EncryptedFileStorage.DomainServices
{
    public class EncryptedFileService : IEncryptedFileService
    {
        private readonly IEncryptedFileInfoRepository _encryptedFileInfoRepository;
        private readonly ILog _log;
        private readonly IEncryptedFileContentRepository _encryptedFileContentRepository;
        private const char ContainerNameSeparator = '/';

        public EncryptedFileService(
            IEncryptedFileInfoRepository encryptedFileInfoRepository, 
            IEncryptedFileContentRepository encryptedFileContentRepository,
            ILogFactory logFactory)
        {
            _encryptedFileInfoRepository = encryptedFileInfoRepository;
            _encryptedFileContentRepository = encryptedFileContentRepository;
            _log = logFactory.CreateLog(this);
        }

        public async Task<Guid> CreateFileInfoAsync(EncryptedFile data)
        {
            ValidatePreparedOriginAndFileName(data);

            var fileInfoExists = await _encryptedFileInfoRepository.FileInfoByOriginAndFileNameExistsAsync(data.Origin, data.FileName);

            if (fileInfoExists)
                throw new DuplicateFileInfoFoundException(
                    $"File info with same file name ('{data.FileName}') and origin ('{data.Origin}') already saved");

            var result = await _encryptedFileInfoRepository.CreateFileInfoAsync(data);

            return result;
        }

        public async Task DeleteFileAsync(Guid fileId)
        {
            var fileInfo = await _encryptedFileInfoRepository.GetFileInfoAsync(fileId);

            if (fileInfo == null)
            {
                throw new FileInfoNotFoundException($"File info for file Id '{fileId}' cannot be found.");
            }

            var deleted = await _encryptedFileInfoRepository.DeleteFileInfoAsync(fileId);

            if (!deleted)
            {
                _log.Warning("File info deletion failed. Could not find file info.", context: fileId);
                return;
            }

            if (fileInfo.IsUploadCompleted)
            {
                await _encryptedFileContentRepository.DeleteContentAsync(fileInfo);

                _log.Info("File content deleted", fileId);
            }
        }

        public async Task<EncryptedFile> GetFileMetadataAsync(Guid fileId)
        {
            return await _encryptedFileInfoRepository.GetFileInfoAsync(fileId);
        }

        public async Task<PaginatedEncryptedFiles> GetPaginatedFilesMetadataAsync(int currentPage, int pageSize)
        {
            ValidatePaging(currentPage, pageSize);

            var skip = (currentPage - 1) * pageSize;
            var take = pageSize;

            var files = await _encryptedFileInfoRepository.GetPaginatedFilesInfoAsync(skip, take);
            var totalCount = await _encryptedFileInfoRepository.GetTotalAsync();

            return new PaginatedEncryptedFiles
            {
                Files = files,
                CurrentPage = currentPage,
                PageSize = pageSize,
                TotalCount = totalCount
            };
        }

        public async Task<PaginatedEncryptedFiles> GetPaginatedFilesMetadataByOriginAsync(int currentPage, int pageSize,
            string origin)
        {
            ValidatePaging(currentPage, pageSize);

            var skip = (currentPage - 1) * pageSize;
            var take = pageSize;

            var files = await _encryptedFileInfoRepository.GetPaginatedFilesInfoByOriginAsync(skip, take, origin);
            var totalCount = await _encryptedFileInfoRepository.GetTotalByOriginAsync(origin);

            return new PaginatedEncryptedFiles
            {
                Files = files,
                CurrentPage = currentPage,
                PageSize = pageSize,
                TotalCount = totalCount
            };
        }

        public string PrepareContainerName(string origin)
        {
            //Can contain only lowercase letters, numbers, and the dash (-) character
            var validCharsFromOrigin = origin.ToLower().ToCharArray()
                .Where(x => (x >= 'a' && x <= 'z') || x == '-');

            var containerName = new string(validCharsFromOrigin.ToArray()).Trim('-');

            return containerName;
        }

        public string PrepareBlobName(string fileName, Guid fileId)
        {
            //Can contain only letters, numbers, dash (-) and underscore (_) character
            var validCharsFromFileName = fileName.ToCharArray()
                .Where(x => (x >= 'a' && x <= 'z') || (x >= 'A' && x <= 'Z') || (x >= '0' && x <= '9') 
                            || x == '-' || x == '_');

            return $"{new string(validCharsFromFileName.ToArray()).Trim('-')}_{fileId}";
        }
        
        internal void ValidatePreparedOriginAndFileName(EncryptedFile data)
        {
            var preparedName = PrepareContainerName(data.Origin);

            if (string.IsNullOrEmpty(preparedName) || preparedName.Length < 3)
            {
                throw new ArgumentException("Origin, when prepared, must not be less then 3 characters in length",
                    nameof(data.Origin));
            }

            if (preparedName.Length > 63)
            {
                throw new ArgumentException("Origin, when prepared, must not be more then 63 characters in length",
                    nameof(data.Origin));
            }

            //We can validate with and empty guid, cause we just need GUID chars
            preparedName = PrepareBlobName(data.FileName, Guid.Empty);

            if (preparedName.Length > 1024)
            {
                throw new ArgumentException("File name, when prepared, must not be more then 1024 characters in length",
                    nameof(data.FileName));
            }
        }

        internal void ValidatePaging(int currentPage, int pageSize)
        {
            if (currentPage < 1)
                throw new ArgumentException("Current page cannot be less than 1", nameof(currentPage));

            if (currentPage > 10_000)
                throw new ArgumentException("Current page cannot exceed more than 10000", nameof(currentPage));

            if (pageSize < 1)
                throw new ArgumentException("Page size can't be less than 1", nameof(pageSize));

            if (pageSize > 500)
                throw new ArgumentException("Page size cannot exceed more then 500", nameof(pageSize));
        }
        
        public async Task<EncryptedFile> EncryptAndStoreFileContentAsync(Guid fileId, IFormFile formFile)
        {
            if (formFile == null)
            {
                throw new FileContentRequiredException("File is required for a upload.");
            }

            var fileInfo = await _encryptedFileInfoRepository.GetFileInfoAsync(fileId);

            if (fileInfo == null)
            {
                throw new FileInfoNotFoundException($"File info for file Id '{fileId}' cannot be found.");
            }

            fileInfo.BlobName = $"{PrepareContainerName(fileInfo.Origin)}{ContainerNameSeparator}{PrepareBlobName(fileInfo.FileName, fileId)}";
            
            await _encryptedFileContentRepository.SaveContentAsync(fileInfo, formFile);

            fileInfo.Length = formFile.Length;
            fileInfo.FileDate = DateTime.UtcNow;
            fileInfo.IsUploadCompleted = true;

            await _encryptedFileInfoRepository.UpdateFileInfoAsync(fileInfo);

            return fileInfo;
        }

        public async Task<Stream> DecryptAndDownloadFileContentAsync(Guid fileId)
        {
            var fileInfo = await _encryptedFileInfoRepository.GetFileInfoAsync(fileId);

            if (fileInfo == null)
            {
                throw new FileInfoNotFoundException($"File info for file Id '{fileId}' cannot be found.");
            }

            return await _encryptedFileContentRepository.GetContentAsync(fileInfo);
        }
    }
}
