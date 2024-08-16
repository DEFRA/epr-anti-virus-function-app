namespace EPR.Antivirus.Application.Exceptions;

using System.Diagnostics.CodeAnalysis;

[ExcludeFromCodeCoverage]
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
}