namespace EPR.Antivirus.Application.Exceptions;

using System.Diagnostics.CodeAnalysis;

[ExcludeFromCodeCoverage]
[Serializable]
public class BlobStorageServiceException : Exception
{
    public BlobStorageServiceException()
    {
    }

    public BlobStorageServiceException(string message)
        : base(message)
    {
    }

    public BlobStorageServiceException(string message, Exception inner)
        : base(message, inner)
    {
    }
}