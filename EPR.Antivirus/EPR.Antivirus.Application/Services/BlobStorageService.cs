namespace EPR.Antivirus.Application.Services;

using Azure.Storage.Blobs;
using Data.Enums;
using Data.Options;
using Exceptions;
using Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

public class BlobStorageService : IBlobStorageService
{
    private readonly BlobServiceClient _blobServiceClient;
    private readonly BlobStorageOptions _options;
    private readonly ILogger<BlobStorageService> _logger;

    public BlobStorageService(BlobServiceClient blobServiceClient, IOptions<BlobStorageOptions> options, ILogger<BlobStorageService> logger)
    {
        _blobServiceClient = blobServiceClient;
        _options = options.Value;
        _logger = logger;
    }

    public static Dictionary<string, string> CreateMetadata(Guid submissionId, Guid userId, FileType fileType, string submissionPeriod)
    {
        return new Dictionary<string, string>
        {
            {
                "fileTypeEPR", fileType.ToString()
            },
            {
                "submissionId", submissionId.ToString()
            },
            {
                "userId", userId.ToString()
            },
            {
                "fileType", "text/csv"
            },
            {
                "submissionPeriod", submissionPeriod
            }
        };
    }

    public async Task<string> UploadFileStreamWithMetadataAsync(
        Stream stream,
        SubmissionType submissionType,
        IDictionary<string, string> metadata)
    {
        try
        {
            var containerName = SetContainerName(submissionType);

            var blobContainerClient = _blobServiceClient.GetBlobContainerClient(containerName);
            var blob = blobContainerClient.GetBlobClient(Guid.NewGuid().ToString());
            await blob.UploadAsync(stream);
            await blob.SetMetadataAsync(metadata);

            _logger.LogInformation("File Uploaded to blob storage");
            return blob.Name;
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Error occurred during uploading to blob storage");
            throw new BlobStorageServiceException("Error occurred during uploading to blob storage", exception);
        }
    }

    private string SetContainerName(SubmissionType submissionType)
    {
        var blobContainerName = string.Empty;

        switch (submissionType)
        {
            case SubmissionType.Producer:
                blobContainerName = _options.PomContainerName;
                break;
            case SubmissionType.Registration:
                blobContainerName = _options.RegistrationContainerName;
                break;
            case SubmissionType.Subsidiary:
                blobContainerName = _options.SusidiaryContainerName;
                break;
            default:
                break;
        }

        return blobContainerName;
    }
}