namespace EPR.Antivirus.Data.DTOs.SubmissionStatusApi;

using System.Diagnostics.CodeAnalysis;
using Enums;

[ExcludeFromCodeCoverage]
public record SubmissionEventRequest(
    Guid FileId,
    string BlobContainerName,
    ScanResult AntivirusScanResult,
    string BlobName,
    bool? RequiresRowValidation,
    EventType Type = EventType.AntivirusResult,
    List<string> Errors = null);
