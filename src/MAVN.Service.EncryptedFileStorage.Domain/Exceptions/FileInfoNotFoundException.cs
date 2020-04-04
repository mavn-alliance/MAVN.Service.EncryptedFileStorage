using System;

namespace MAVN.Service.EncryptedFileStorage.Domain.Exceptions
{
    public class FileInfoNotFoundException: Exception
    {
        public FileInfoNotFoundException(string message): base(message)
        {
        }
    }
}
