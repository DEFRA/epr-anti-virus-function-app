namespace EPR.Antivirus.Application.Exceptions;

using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;

[ExcludeFromCodeCoverage]
[Serializable]
public class SubmissionStatusApiClientException : Exception
{
    public SubmissionStatusApiClientException()
    {
    }

    public SubmissionStatusApiClientException(string message)
        : base(message)
    {
    }

    public SubmissionStatusApiClientException(string message, Exception inner)
        : base(message, inner)
    {
    }

    protected SubmissionStatusApiClientException(SerializationInfo info, StreamingContext context)
        : base(info, context)
    {
    }
}