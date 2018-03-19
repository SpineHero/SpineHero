using System;

namespace SpineHero.Utils.CloudStorage
{
    public class CloudStorageException : Exception
    {
        public CloudStorageException()
        { }

        public CloudStorageException(string message) : base(message)
        { }

        public CloudStorageException(string message, Exception innerException) : base(message, innerException)
        { }
    }
}