using System;
using System.IO;
using System.Threading.Tasks;
using Lykke.Service.EncryptedFileStorage.Domain.Models;
using Microsoft.AspNetCore.Http;

namespace Lykke.Service.EncryptedFileStorage.Domain.Repositories
{
    public interface IEncryptedFileContentRepository
    {
        Task SaveContentAsync(EncryptedFile fileInfo, IFormFile formFile);

        Task<Stream> GetContentAsync(EncryptedFile fileInfo);

        Task DeleteContentAsync(EncryptedFile fileInfo);
    }
}
