namespace EPR.Antivirus.Application.Exceptions;

using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;

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

    protected ServiceBusQueueClientException(SerializationInfo info, StreamingContext context)
        : base(info, context)
    {
    }
}