using System;

namespace Lykke.Service.EncryptedFileStorage.Domain.Exceptions
{
    public class DuplicateFileInfoFoundException : Exception
    {
        public DuplicateFileInfoFoundException(string message) : base(message)
        {
        }
    }
}
