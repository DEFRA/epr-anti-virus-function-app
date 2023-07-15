namespace EPR.Antivirus.Application.Exceptions;

using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;

[ExcludeFromCodeCoverage]
[Serializable]
public class AntivirusServiceException : Exception
{
    public AntivirusServiceException()
    {
    }

    public AntivirusServiceException(string message)
        : base(message)
    {
    }

    public AntivirusServiceException(string message, Exception inner)
        : base(message, inner)
    {
    }

    protected AntivirusServiceException(SerializationInfo info, StreamingContext context)
        : base(info, context)
    {
    }
}