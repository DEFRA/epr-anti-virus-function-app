namespace EPR.Antivirus.Application.Clients;

using System.Text.Json;
using Azure.Messaging.ServiceBus;
using Data.DTOs.ServiceBusQueue;
using Data.Enums;
using Data.Options;
using Exceptions;
using Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

public class ServiceBusQueueClient : IServiceBusQueueClient
{
    private readonly ServiceBusClient _serviceBusClient;
    private readonly ServiceBusOptions _options;
    private readonly ILogger<ServiceBusQueueClient> _logger;

    public ServiceBusQueueClient(ServiceBusClient serviceBusClient, IOptions<ServiceBusOptions> options, ILogger<ServiceBusQueueClient> logger)
    {
        _serviceBusClient = serviceBusClient;
        _options = options.Value;
        _logger = logger;
    }

    public async Task SendMessageAsync(SubmissionType submissionType, ServiceBusQueueMessage serviceBusQueueMessage)
    {
        try
        {
            var queueName = GetQueueName(submissionType);

            var serializedMessage = JsonSerializer.Serialize(serviceBusQueueMessage);
            var serviceBusMessage = new ServiceBusMessage(serializedMessage);
            var sender = _serviceBusClient.CreateSender(queueName);
            await sender.SendMessageAsync(serviceBusMessage);

            _logger.LogInformation("Message posted to service bus");
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Failed to post message to service bus");
            throw new ServiceBusQueueClientException("Failed to post message to service bus", exception);
        }
    }

    private string GetQueueName(SubmissionType submissionType) =>
        submissionType == SubmissionType.Producer
            ? _options.PomUploadQueueName
            : _options.RegistrationDataQueueName;
}