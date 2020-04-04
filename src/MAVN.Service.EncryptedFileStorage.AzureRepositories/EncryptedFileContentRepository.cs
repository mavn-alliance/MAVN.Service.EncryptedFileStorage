using System.IO;
using System.Threading;
using System.Threading.Tasks;
using AzureStorage;
using MAVN.Service.EncryptedFileStorage.Domain.Models;
using MAVN.Service.EncryptedFileStorage.Domain.Repositories;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.KeyVault;
using Microsoft.Azure.Storage.Blob;

namespace MAVN.Service.EncryptedFileStorage.AzureRepositories
{
    public class EncryptedFileContentRepository : IEncryptedFileContentRepository
    {
        private readonly string _fileStorageKey;
        private readonly CloudBlobClient _cloudBlobClient;
        private readonly IBlobStorage _blobStorage;
        private readonly KeyVaultKeyResolver _cloudResolver;
        private const char ContainerNameSeparator = '/';

        public EncryptedFileContentRepository(
            string fileStorageKey,
            CloudBlobClient cloudBlobClient,
            IKeyVaultClient keyVaultClient,
            IBlobStorage blobStorage)
        {
            _fileStorageKey = fileStorageKey;
            _cloudBlobClient = cloudBlobClient;
            _blobStorage = blobStorage;
            _cloudResolver = new KeyVaultKeyResolver(keyVaultClient);
        }

        public async Task SaveContentAsync(EncryptedFile fileInfo, IFormFile formFile)
        {
            if (fileInfo == null)
            {
                return;
            }

            var blobParts = fileInfo.BlobName.Split(ContainerNameSeparator);
            var containerName = blobParts[0];
            var blobName = blobParts[1];

            var contain = _cloudBlobClient.GetContainerReference(containerName);
            contain.CreateIfNotExists();

            var rsa = await _cloudResolver.ResolveKeyAsync(
                _fileStorageKey,
                CancellationToken.None);

            var policy = new BlobEncryptionPolicy(rsa, null);

            var options = new BlobRequestOptions { EncryptionPolicy = policy };

            var blob = contain.GetBlockBlobReference(blobName);

            using (var stream = formFile.OpenReadStream())
            {
                // Options object is containing all information about the 
                await blob.UploadFromStreamAsync(stream, stream.Length, null, options, null);
            }
        }

        public async Task<Stream> GetContentAsync(EncryptedFile fileInfo)
        {
            if (fileInfo.BlobName == null)
            {
                return null;
            }

            var blobParts = fileInfo.BlobName.Split(ContainerNameSeparator);
            var containerName = blobParts[0];
            var blobName = blobParts[1];

            var contain = _cloudBlobClient.GetContainerReference(containerName);
            contain.CreateIfNotExists();

            var policy = new BlobEncryptionPolicy(null, _cloudResolver);
            var options = new BlobRequestOptions { EncryptionPolicy = policy };

            var blob = contain.GetBlockBlobReference(blobName);

            // Options object is containing all information about the 
            return await blob.OpenReadAsync(null, options, null);
        }

        public async Task DeleteContentAsync(EncryptedFile fileInfo)
        {
            if (fileInfo == null)
            {
                return;
            }

            var blobParts = fileInfo.BlobName.Split(ContainerNameSeparator);
            var containerName = blobParts[0];

            var blobKey = fileInfo.FileId.ToString();

            var blobExists = await _blobStorage.HasBlobAsync(containerName, blobKey);

            if (blobExists)
                await _blobStorage.DelBlobAsync(containerName, blobKey);
        }
    }
}
