namespace EPR.Antivirus.Data.Options;

using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

[ExcludeFromCodeCoverage]
public class BlobStorageOptions
{
    public const string Section = "BlobStorage";

    [Required]
    public string ConnectionString { get; init; }

    [Required]
    public string PomContainerName { get; init; }

    [Required]
    public string RegistrationContainerName { get; init; }
}