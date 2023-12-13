namespace EPR.Antivirus.Data.Options;

using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

[ExcludeFromCodeCoverage]
public class ServiceBusOptions
{
    public const string Section = "ServiceBus";

    [Required]
    public string ConnectionString { get; set; }

    [Required]
    public string PomUploadQueueName { get; set; }

    [Required]
    public string RegistrationDataQueueName { get; set; }
}