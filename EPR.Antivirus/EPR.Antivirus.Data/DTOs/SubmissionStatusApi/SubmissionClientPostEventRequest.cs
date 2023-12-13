namespace EPR.Antivirus.Data.DTOs.SubmissionStatusApi;

using System.Diagnostics.CodeAnalysis;
using Enums;

[ExcludeFromCodeCoverage]
public record SubmissionClientPostEventRequest(
    Guid OrganisationId,
    Guid UserId,
    Guid SubmissionId,
    FileType FileType,
    string BlobName,
    Guid FileId,
    ScanResult ScanResult,
    List<string> Errors);