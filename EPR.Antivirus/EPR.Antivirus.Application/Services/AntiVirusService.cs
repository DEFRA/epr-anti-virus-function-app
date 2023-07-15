namespace EPR.Antivirus.Application.Services;

using Clients.Interfaces;
using Data.Constants;
using Data.DTOs.ServiceBusQueue;
using Data.DTOs.TradeAntivirusQueue;
using Data.Enums;
using Extensions;
using Interfaces;
using Microsoft.Extensions.Logging;

public class AntivirusService : IAntivirusService
{
    private readonly ISubmissionStatusApiClient _submissionStatusApiClient;
    private readonly IServiceBusQueueClient _serviceBusQueueClient;
    private readonly IBlobStorageService _blobStorageService;
    private readonly ITradeAntivirusApiClient _tradeAntivirusApiClient;
    private readonly ILogger<AntivirusService> _logger;

    public AntivirusService(
        ISubmissionStatusApiClient submissionStatusApiClient,
        IServiceBusQueueClient serviceBusQueueClient,
        IBlobStorageService blobStorageService,
        ITradeAntivirusApiClient tradeAntivirusApiClient,
        ILogger<AntivirusService> logger)
    {
        _submissionStatusApiClient = submissionStatusApiClient;
        _serviceBusQueueClient = serviceBusQueueClient;
        _blobStorageService = blobStorageService;
        _tradeAntivirusApiClient = tradeAntivirusApiClient;
        _logger = logger;
    }

    public async Task HandleAsync(TradeAntivirusQueueResult message)
    {
        var submissionFile = await _submissionStatusApiClient.GetSubmissionFileAsync(message.Key);
        var blobId = string.Empty;
        var errors = new List<string>();

        if (message.Status == ScanResult.Success)
        {
            _logger.LogInformation("Antivirus check was successful");

            blobId = await ProcessFileAsync(
                submissionFile.SubmissionId,
                submissionFile.SubmissionType,
                submissionFile.FileId,
                submissionFile.FileType,
                submissionFile.OrganisationId,
                submissionFile.UserId,
                message.Collection);
        }
        else
        {
            errors.Add(ErrorCodes.UploadedFileContainsThreat);
            _logger.LogWarning("Antivirus check did not indicate success. Received {}", message.Status.ToString());
        }

        await _submissionStatusApiClient.PostEventAsync(
                submissionFile.OrganisationId,
                submissionFile.UserId,
                submissionFile.SubmissionId,
                blobId,
                submissionFile.FileId,
                message.Status,
                errors);
    }

    private async Task<string> ProcessFileAsync(
        Guid submissionId,
        SubmissionType submissionType,
        Guid fileId,
        FileType fileType,
        Guid organisationId,
        Guid userId,
        string collection)
    {
        var fileStream = await _tradeAntivirusApiClient.GetFileAsync(collection, fileId);
        var fileMetadata = BlobStorageService.CreateMetadata(submissionId, userId, fileType);
        var blobId = await _blobStorageService.UploadFileStreamWithMetadataAsync(fileStream, submissionType, fileMetadata);
        var submissionSubType = fileType.ToSubmissionSubType();
        var serviceBusQueueMessage = new ServiceBusQueueMessage
        {
            BlobName = blobId,
            SubmissionId = submissionId,
            SubmissionSubType = submissionSubType,
            OrganisationId = organisationId,
            UserId = userId
        };

        await _serviceBusQueueClient.SendMessageAsync(submissionType, serviceBusQueueMessage);

        return blobId;
    }
}