using System;
using System.IO;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Lykke.Service.EncryptedFileStorage.Client.Models.Requests;
using Lykke.Service.EncryptedFileStorage.Client.Models.Responses;
using Refit;

namespace Lykke.Service.EncryptedFileStorage.Client
{
    /// <summary>
    /// EncryptedFileStorage client API interface.
    /// </summary>
    [PublicAPI]
    public interface IEncryptedFileStorageApi
    {
        /// <summary>
        /// Save file info
        /// </summary>
        /// <param name="model"><see cref="CreateFileInfoRequest"/></param>
        /// <returns>Id of the newly created encrypted file</returns>
        [Post("/api/files")]
        Task<Guid> CreateFileInfoAsync([Body] CreateFileInfoRequest model);

        /// <summary>
        /// Delete file info and content
        /// </summary>
        /// <param name="fileId">File id</param>
        /// <returns></returns>
        [Delete("/api/files/{fileId}")]
        Task DeleteFileAsync(Guid fileId);

        /// <summary>
        /// Get file info
        /// </summary>
        /// <param name="fileId">File id</param>
        /// <returns></returns>
        [Get("/api/files/{fileId}")]
        Task<FileInfoResponse> GetFileInfoAsync(Guid fileId);

        /// <summary>
        /// Encrypt and store file
        /// </summary>
        /// <param name="fileId">The If of the file info</param>
        /// <param name="file"> the file</param>
        [Multipart]
        [Post("/api/files/{fileId}/content")]
        Task<FileInfoResponse> EncryptAndStoreFileContentAsync(Guid fileId, [AliasAs("file")] StreamPart stream);

        /// <summary>
        /// Downloads an encrypted file
        /// </summary>
        /// <param name="fileId">The id of the file info.</param>
        /// <returns>Id of the newly created encrypted file</returns>
        [Get("/api/files/{fileId}/content")]
        Task<Stream> DecryptAndDownloadFileContentAsync(Guid fileId);

        /// <summary>
        /// Get paginated files info
        /// </summary>
        /// <param name="pagingInfo"><see cref="PaginationModelRequest"/></param>
        /// <returns><see cref="PaginatedFilesInfoResponse"/></returns>
        [Get("/api/files")]
        Task<PaginatedFilesInfoResponse> GetFilesInfoAsync([Query] PaginationModelRequest pagingInfo);

        /// <summary>
        /// Get paginated files info based on their origin
        /// </summary>
        /// <param name="pagingInfo"><see cref="PaginationModelRequest"/></param>
        /// <returns><see cref="PaginatedFilesInfoResponse"/></returns>
        [Get("/api/files/origin/{origin}")]
        Task<PaginatedFilesInfoResponse> GetFilesInfoByOriginAsync(string origin,
            [Query] PaginationModelRequest pagingInfo);
    }
}
