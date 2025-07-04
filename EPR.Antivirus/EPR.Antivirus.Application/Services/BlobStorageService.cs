namespace EPR.Antivirus.Application.Services;

using Azure.Storage.Blobs;
using Data.Enums;
using Data.Options;
using Exceptions;
using Interfaces;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

public class BlobStorageService : IBlobStorageService
{
    private readonly BlobServiceClient _blobServiceClient;
    private readonly BlobStorageOptions _options;
    private readonly ILogger<BlobStorageService> _logger;

    public BlobStorageService(
        BlobServiceClient blobServiceClient,
        IOptions<BlobStorageOptions> options,
        ILogger<BlobStorageService> logger)
    {
        _blobServiceClient = blobServiceClient;
        _options = options.Value;
        _logger = logger;
    }

    public static Dictionary<string, string> CreateMetadata(
        Guid submissionId,
        Guid userId,
        FileType fileType,
        string submissionPeriod,
        string fileName,
        Guid organisationId,
        Guid? complianceSchemeId = null)
    {
        var contentType = fileType switch
        {
            FileType.Accreditation => GetContentType(fileName),
            _ => "text/csv"
        };

        var metaData = new Dictionary<string, string>
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
                "fileType", contentType
            }
        };

        switch (fileType)
        {
            case FileType.Subsidiaries:
            case FileType.CompaniesHouse:
            case FileType.Accreditation:
                metaData.Add("fileName", fileName);
                metaData.Add("organisationId", organisationId.ToString());
                break;
            default:
                metaData.Add("submissionPeriod", submissionPeriod);
                break;
        }

        if (fileType is FileType.Subsidiaries && complianceSchemeId is not null)
        {
            metaData.Add("complianceSchemeId", complianceSchemeId.ToString());
        }

        return metaData;
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

    private static string GetContentType(string fileName)
    {
        var provider = new FileExtensionContentTypeProvider();

        if (!provider.TryGetContentType(fileName, out string contentType))
        {
            contentType = "application/octet-stream";
        }

        return contentType;
    }

    private string SetContainerName(SubmissionType submissionType) => submissionType switch
    {
        SubmissionType.Producer => _options.PomContainerName,
        SubmissionType.Registration => _options.RegistrationContainerName,
        SubmissionType.Subsidiary => _options.SubsidiaryContainerName,
        SubmissionType.CompaniesHouse => _options.SubsidiaryCompaniesHouseContainerName,
        SubmissionType.Accreditation => _options.AccreditationContainerName,
        _ => throw new InvalidOperationException()
    };
}