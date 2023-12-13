namespace EPR.Antivirus.Application.Services;

using Clients.Interfaces;
using Common.Logging.Constants;
using Common.Logging.Models;
using Common.Logging.Services;
using Data.Constants;
using Data.DTOs.AntiVirusService;
using Data.DTOs.ServiceBusQueue;
using Data.DTOs.SubmissionStatusApi;
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
    private readonly ILoggingService _loggingService;
    private readonly ITradeAntivirusApiClient _tradeAntivirusApiClient;
    private readonly ILogger<AntivirusService> _logger;

    public AntivirusService(
        ISubmissionStatusApiClient submissionStatusApiClient,
        IServiceBusQueueClient serviceBusQueueClient,
        IBlobStorageService blobStorageService,
        ILoggingService loggingService,
        ITradeAntivirusApiClient tradeAntivirusApiClient,
        ILogger<AntivirusService> logger)
    {
        _submissionStatusApiClient = submissionStatusApiClient;
        _serviceBusQueueClient = serviceBusQueueClient;
        _blobStorageService = blobStorageService;
        _loggingService = loggingService;
        _tradeAntivirusApiClient = tradeAntivirusApiClient;
        _logger = logger;
    }

    public async Task HandleAsync(TradeAntivirusQueueResult message)
    {
        Guid fileId = message.Key;
        bool checkIsSuccessful = message.Status == ScanResult.Success;
        SubmissionFileResult? submissionFile = await _submissionStatusApiClient.GetSubmissionFileAsync(fileId);
        var blobName = string.Empty;
        var errors = new List<string>();

        try
        {
            await LogAsync(submissionFile, checkIsSuccessful);
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "An error occurred creating the protective monitoring event");
        }

        if (checkIsSuccessful)
        {
            blobName = await ProcessFileAsync(
                new ProcessFileAsyncRequest(submissionFile.SubmissionId,
                submissionFile.SubmissionType,
                submissionFile.SubmissionPeriod,
                submissionFile.FileId,
                submissionFile.FileType,
                submissionFile.OrganisationId,
                submissionFile.UserId,
                message.Collection));
        }
        else
        {
            errors.Add(ErrorCodes.UploadedFileContainsThreat);
        }

        await _submissionStatusApiClient.PostEventAsync(
            new SubmissionClientPostEventRequest(
                submissionFile.OrganisationId,
                submissionFile.UserId,
                submissionFile.SubmissionId,
                submissionFile.FileType,
                blobName,
                submissionFile.FileId,
                message.Status,
                errors));
    }

    private async Task<string> ProcessFileAsync(ProcessFileAsyncRequest processFileAsyncRequest)
    {
        var fileStream = await _tradeAntivirusApiClient.GetFileAsync(processFileAsyncRequest.Collection, processFileAsyncRequest.FileId);
        var fileMetadata = BlobStorageService.CreateMetadata(
            processFileAsyncRequest.SubmissionId,
            processFileAsyncRequest.UserId,
            processFileAsyncRequest.FileType,
            processFileAsyncRequest.SubmissionPeriod);
        var blobName = await _blobStorageService.UploadFileStreamWithMetadataAsync(
            fileStream, processFileAsyncRequest.SubmissionType, fileMetadata);
        var submissionSubType = processFileAsyncRequest.FileType.ToSubmissionSubType();
        var serviceBusQueueMessage = new ServiceBusQueueMessage(
            blobName,
            processFileAsyncRequest.SubmissionId,
            submissionSubType,
            processFileAsyncRequest.OrganisationId,
            processFileAsyncRequest.UserId,
            processFileAsyncRequest.SubmissionPeriod);

        await _serviceBusQueueClient.SendMessageAsync(processFileAsyncRequest.SubmissionType, serviceBusQueueMessage);

        return blobName;
    }

    private async Task LogAsync(SubmissionFileResult submissionFile, bool checkIsSuccessful)
    {
        int priority = checkIsSuccessful ? Priorities.NormalEvent : Priorities.ExceptionEvent;
        string transactionCode = checkIsSuccessful ? TransactionCodes.AntivirusCleanUpload : TransactionCodes.AntivirusThreatDetected;
        var logMessage = $"Antivirus check {(checkIsSuccessful ? "was successful" : "did not indicate success")} for file '{submissionFile.FileName}'";
        var additionalInfo = $"FileId: '{submissionFile.FileId}'";

        await _loggingService.SendEventAsync(
            submissionFile.UserId,
            new ProtectiveMonitoringEvent(
                submissionFile.SubmissionId,
                "epr-anti-virus-function-app",
                PmcCodes.Code0203,
                priority,
                transactionCode,
                logMessage,
                additionalInfo));
    }
}