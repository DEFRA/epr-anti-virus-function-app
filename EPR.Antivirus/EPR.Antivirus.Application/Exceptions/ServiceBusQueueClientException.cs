namespace EPR.Antivirus.Application.Exceptions;

using System.Diagnostics.CodeAnalysis;

[ExcludeFromCodeCoverage]
[Serializable]
public class ServiceBusQueueClientException : Exception
{
    public ServiceBusQueueClientException()
    {
    }

    public ServiceBusQueueClientException(string message)
        : base(message)
    {
    }

    public ServiceBusQueueClientException(string message, Exception inner)
        : base(message, inner)
    {
    }
}