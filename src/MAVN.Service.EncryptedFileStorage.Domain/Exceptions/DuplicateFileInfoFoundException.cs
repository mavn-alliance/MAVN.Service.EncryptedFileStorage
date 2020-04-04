using System;

namespace MAVN.Service.EncryptedFileStorage.Domain.Exceptions
{
    public class DuplicateFileInfoFoundException : Exception
    {
        public DuplicateFileInfoFoundException(string message) : base(message)
        {
        }
    }
}
