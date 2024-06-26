namespace EPR.Antivirus.Application.Exceptions;

using System.Diagnostics.CodeAnalysis;

[ExcludeFromCodeCoverage]
[Serializable]
public class TradeAntivirusApiClientException : Exception
{
    public TradeAntivirusApiClientException()
    {
    }

    public TradeAntivirusApiClientException(string message)
        : base(message)
    {
    }

    public TradeAntivirusApiClientException(string message, Exception inner)
        : base(message, inner)
    {
    }
}