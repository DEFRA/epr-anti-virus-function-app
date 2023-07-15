namespace EPR.Antivirus.Application.Exceptions;

using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;

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

    protected TradeAntivirusApiClientException(SerializationInfo info, StreamingContext context)
        : base(info, context)
    {
    }
}