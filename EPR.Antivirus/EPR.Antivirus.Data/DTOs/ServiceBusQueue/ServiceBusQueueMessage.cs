namespace EPR.Antivirus.Data.DTOs.ServiceBusQueue;

using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;
using Enums;

[ExcludeFromCodeCoverage]
public record ServiceBusQueueMessage(
    string BlobName,
    Guid SubmissionId,
    [property: JsonConverter(typeof(JsonStringEnumConverter))] SubmissionSubType? SubmissionSubType,
    Guid OrganisationId,
    Guid UserId,
    string SubmissionPeriod);