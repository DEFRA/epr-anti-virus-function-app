namespace EPR.Antivirus.Application.Exceptions;

using System.Diagnostics.CodeAnalysis;

[ExcludeFromCodeCoverage]
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
}