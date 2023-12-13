namespace EPR.Antivirus.Application.Clients.Interfaces;

using Data.DTOs.SubmissionStatusApi;

public interface ISubmissionStatusApiClient
{
    Task PostEventAsync(SubmissionClientPostEventRequest request);

    Task<SubmissionFileResult?> GetSubmissionFileAsync(Guid fileId);
}