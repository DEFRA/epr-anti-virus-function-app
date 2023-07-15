namespace EPR.Antivirus.Application.Clients.Interfaces;

using Data.DTOs.ServiceBusQueue;
using Data.Enums;

public interface IServiceBusQueueClient
{
    Task SendMessageAsync(SubmissionType submissionType, ServiceBusQueueMessage serviceBusQueueMessage);
}