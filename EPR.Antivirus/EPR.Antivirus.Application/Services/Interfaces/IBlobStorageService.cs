namespace EPR.Antivirus.Application.Services.Interfaces;

using Data.Enums;

public interface IBlobStorageService
{
     Task<string> UploadFileStreamWithMetadataAsync(Stream stream, SubmissionType submissionType, IDictionary<string, string> metadata);
}