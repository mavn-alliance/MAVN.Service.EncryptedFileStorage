using System;

namespace Lykke.Service.EncryptedFileStorage.Domain.Exceptions
{
    public class FileInfoNotFoundException: Exception
    {
        public FileInfoNotFoundException(string message): base(message)
        {
        }
    }
}
