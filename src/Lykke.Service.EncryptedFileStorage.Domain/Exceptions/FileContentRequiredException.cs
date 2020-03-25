using System;

namespace Lykke.Service.EncryptedFileStorage.Domain.Exceptions
{
    public class FileContentRequiredException : Exception
    {
        public FileContentRequiredException(string message): base(message)
        {
        }
    }
}
