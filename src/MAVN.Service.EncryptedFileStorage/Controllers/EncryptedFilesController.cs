using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using AutoMapper;
using Common.Log;
using Lykke.Common.Api.Contract.Responses;
using Lykke.Common.ApiLibrary.Exceptions;
using Lykke.Common.Log;
using MAVN.Service.EncryptedFileStorage.Auth;
using MAVN.Service.EncryptedFileStorage.Client;
using MAVN.Service.EncryptedFileStorage.Client.Models.Requests;
using MAVN.Service.EncryptedFileStorage.Client.Models.Responses;
using MAVN.Service.EncryptedFileStorage.Domain.Exceptions;
using MAVN.Service.EncryptedFileStorage.Domain.Models;
using MAVN.Service.EncryptedFileStorage.Domain.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;
using RabbitMQ.Client;
using Refit;
using Swashbuckle.AspNetCore.Annotations;

namespace MAVN.Service.EncryptedFileStorage.Controllers
{
    [Microsoft.AspNetCore.Authorization.Authorize]
    [ApiController]
    [Route("api/files")]
    public class EncryptedFilesController : ControllerBase
    {
        private readonly IEncryptedFileService _encryptedFileService;
        private readonly IApiKeyService _apiKeyService;
        private readonly ILog _log;
        private readonly IMapper _mapper;

        public EncryptedFilesController(IEncryptedFileService encryptedFileService, IApiKeyService apiKeyService,
            ILogFactory logFactory, IMapper mapper)
        {
            _encryptedFileService = encryptedFileService;
            _apiKeyService = apiKeyService;
            _mapper = mapper;
            _log = logFactory.CreateLog(this);
        }

        /// <summary>
        /// Save file info
        /// </summary>
        /// <param name="model"><see cref="CreateFileInfoRequest"/></param>
        /// <returns>Id of the newly created encrypted file info</returns>
        [HttpPost]
        [SwaggerOperation("Save file info")]
        [ProducesResponseType(typeof(Guid), (int) HttpStatusCode.OK)]
        [ProducesResponseType((int) HttpStatusCode.BadRequest)]
        public async Task<Guid> CreateFileInfoAsync(CreateFileInfoRequest model)
        {
            _log.Info("Save file info started", model);

            var data = _mapper.Map<EncryptedFile>(model);

            Guid result;

            try
            {
                result = await _encryptedFileService.CreateFileInfoAsync(data);
            }
            catch (DuplicateFileInfoFoundException e)
            {
                _log.Error(e, e.Message, model);
                throw new BadRequestException(e.Message);
            }

            _log.Info("Save file info finished", model);

            return result;
        }

        /// <summary>
        /// Delete file info and content
        /// </summary>
        /// <param name="fileId">File id</param>
        /// <returns></returns>
        [HttpDelete("/api/files/{fileId}")]
        [SwaggerOperation("Delete file info and content")]
        [ProducesResponseType((int) HttpStatusCode.NoContent)]
        public async Task DeleteFileAsync([Required] [FromRoute] Guid fileId)
        {
            _log.Info("File deletion started", fileId);

            try
            {
                await _encryptedFileService.DeleteFileAsync(fileId);
            }
            catch (FileInfoNotFoundException e)
            {
                _log.Error(e, e.Message, fileId);
                throw new BadRequestException(e.Message);
            }
            
            _log.Info("File deletion finished", fileId);
        }

        /// <summary>
        /// Get file info
        /// </summary>
        /// <param name="fileId">File id</param>
        /// <returns></returns>
        [HttpGet("/api/files/{fileId}")]
        [SwaggerOperation("Get file info")]
        [ProducesResponseType((int) HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        public async Task<FileInfoResponse> GetFileInfoAsync([Required] [FromRoute] Guid fileId)
        {
            _log.Info("Get file info started", fileId);

            var result = await _encryptedFileService.GetFileMetadataAsync(fileId);

            if (result != null)
                _log.Info($"{GetApiKeyMessage()} to get file info. File id is in context.", result.FileId);

            _log.Info("Get file info finished", fileId);

            return _mapper.Map<FileInfoResponse>(result);
        }

        /// <summary>
        /// Get paginated files info
        /// </summary>
        /// <param name="pagingInfo"><see cref="PaginationModelRequest"/></param>
        /// <returns><see cref="PaginatedFilesInfoResponse"/></returns>
        [HttpGet("/api/files")]
        [SwaggerOperation("Get paginated files info")]
        [ProducesResponseType((int) HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        public async Task<PaginatedFilesInfoResponse> GetFilesInfoAsync(
            [FromQuery] PaginationModelRequest pagingInfo)
        {
            var result =
                await _encryptedFileService.GetPaginatedFilesMetadataAsync(pagingInfo.CurrentPage, pagingInfo.PageSize);

            if (result.Files.Any())
            {
                var fileIds = result.Files.Select(x => x.FileId);
                _log.Info($"{GetApiKeyMessage()} to get paginated files info. File ids are in context.", new {fileIds});
            }

            return _mapper.Map<PaginatedFilesInfoResponse>(result);
        }

        /// <summary>
        /// Get paginated files info based on their origin
        /// </summary>
        /// <param name="pagingInfo"><see cref="PaginationModelRequest"/></param>
        /// <returns><see cref="PaginatedFilesInfoResponse"/></returns>
        [HttpGet("/api/files/origin/{origin}")]
        [SwaggerOperation("Get paginated files info based on their origin")]
        [ProducesResponseType((int) HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        public async Task<PaginatedFilesInfoResponse> GetFilesInfoByOriginAsync(
            [Required] [FromRoute] string origin,
            [FromQuery] PaginationModelRequest pagingInfo)
        {
            var result =
                await _encryptedFileService.GetPaginatedFilesMetadataByOriginAsync(pagingInfo.CurrentPage,
                    pagingInfo.PageSize, origin);

            if (result.Files.Any())
            {
                var fileIds = result.Files.Select(x => x.FileId);
                _log.Info($"{GetApiKeyMessage()} to get paginated files info based on origin. File ids are in context.",
                    new {fileIds});
            }

            return _mapper.Map<PaginatedFilesInfoResponse>(result);
        }

        /// <summary>
        /// Encrypt and store file
        /// </summary>
        /// <param name="fileId">The If of the file info</param>
        /// <param name="file"> the file</param>
        [Consumes("multipart/form-data")]
        [HttpPost("{fileId}/content")]
        [RequestFormLimits(MultipartBodyLengthLimit = 1073741274)]
        [RequestSizeLimit(1073741274)]
        public async Task<FileInfoResponse> EncryptAndStoreFileContentAsync(Guid fileId, [FromForm] IFormFile file)
        {
            if (file == null)
            {
                throw new BadRequestException("File is required for a upload.");
            }

            EncryptedFile result;

            try
            {
                result = await _encryptedFileService.EncryptAndStoreFileContentAsync(fileId, file);
            }
            catch (FileInfoNotFoundException e)
            {
                _log.Error(e);
                throw new BadRequestException(e.Message);
            }
            catch (FileContentRequiredException e)
            {
                _log.Error(e);
                throw new BadRequestException(e.Message);
            }

            return _mapper.Map<FileInfoResponse>(result);
        }

        /// <summary>
        /// Downloads an encrypted file
        /// </summary>
        /// <param name="fileId">The id of the file info.</param>
        /// <returns>Id of the newly created encrypted file</returns>
        [HttpGet("{fileId}/content")]
        public async Task<Stream> DecryptAndDownloadFileContentAsync(Guid fileId)
        {
            Stream content;

            try
            {
                content = await _encryptedFileService.DecryptAndDownloadFileContentAsync(fileId);
            }
            catch (FileInfoNotFoundException e)
            {
                _log.Error(e);
                throw new BadRequestException(e.Message);
            }

            Request.Headers.Add("Content-Type", "application/octet-stream");

            return content;
        }

        private string GetApiKeyMessage()
        {
            var apiKey = Request.Headers[KeyAuthOptions.DefaultHeaderName];

            if (string.IsNullOrEmpty(apiKey))
                throw new InvalidOperationException("Api Key is null");

            var apiKeyName = _apiKeyService.GetKeyName(apiKey);

            if (string.IsNullOrEmpty(apiKeyName))
                throw new InvalidOperationException("Api key name does not exists for such key");

            return $"API key '{apiKeyName}' is used";
        }
    }
}
