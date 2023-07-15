namespace EPR.Antivirus.Data.DTOs.ServiceBusQueue;

using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;
using Enums;

[ExcludeFromCodeCoverage]
public class ServiceBusQueueMessage
{
    public string BlobName { get; set; }

    public Guid SubmissionId { get; set; }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public SubmissionSubType? SubmissionSubType { get; set; }

    public Guid OrganisationId { get; set; }

    public Guid UserId { get; set; }
}