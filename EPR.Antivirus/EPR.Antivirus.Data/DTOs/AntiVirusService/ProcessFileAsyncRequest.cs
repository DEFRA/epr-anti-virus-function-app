namespace EPR.Antivirus.Data.DTOs.AntiVirusService;

using System.Diagnostics.CodeAnalysis;
using Enums;

[ExcludeFromCodeCoverage]
public record ProcessFileAsyncRequest(
   Guid SubmissionId,
   SubmissionType SubmissionType,
   string SubmissionPeriod,
   Guid FileId,
   FileType FileType,
   Guid OrganisationId,
   Guid UserId,
   string Collection,
   Guid? ComplianceSchemeId,
   bool? RequiresRowValidation);