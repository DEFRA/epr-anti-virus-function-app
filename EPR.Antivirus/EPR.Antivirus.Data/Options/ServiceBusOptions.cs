namespace EPR.Antivirus.Data.Options;

using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

[ExcludeFromCodeCoverage]
public class ServiceBusOptions
{
    public const string Section = "ServiceBus";

    [Required]
    public string ConnectionString { get; init; }

    [Required]
    public string PomUploadQueueName { get; init; }

    [Required]
    public string RegistrationDataQueueName { get; init; }
}