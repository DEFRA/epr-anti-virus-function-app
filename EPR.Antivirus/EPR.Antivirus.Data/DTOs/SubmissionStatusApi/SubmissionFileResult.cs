namespace EPR.Antivirus.Data.DTOs.SubmissionStatusApi;

using System.Diagnostics.CodeAnalysis;
using Enums;

[ExcludeFromCodeCoverage]
public record SubmissionFileResult(
    Guid SubmissionId,
    SubmissionType SubmissionType,
    Guid FileId,
    string FileName,
    FileType FileType,
    Guid OrganisationId,
    Guid UserId,
    string SubmissionPeriod);