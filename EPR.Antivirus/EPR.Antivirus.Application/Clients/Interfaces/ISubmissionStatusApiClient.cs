namespace EPR.Antivirus.Application.Clients.Interfaces;

using Data.DTOs.SubmissionStatusApi;
using Data.Enums;

public interface ISubmissionStatusApiClient
{
    Task PostEventAsync(
        Guid organisationId, Guid userId, Guid submissionId, string blobName, Guid fileId, ScanResult scanResult, List<string> errors);

    Task<SubmissionFileResult?> GetSubmissionFileAsync(Guid fileId);
}