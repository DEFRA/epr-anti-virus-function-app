namespace EPR.Antivirus.Data.Options;

using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

[ExcludeFromCodeCoverage]
public class BlobStorageOptions
{
    public const string Section = "BlobStorage";

    [Required]
    public string ConnectionString { get; set; }

    [Required]
    public string PomContainerName { get; set; }

    [Required]
    public string RegistrationContainerName { get; set; }

    public string SubsidiaryContainerName { get; set; }

    public string SubsidiaryCompaniesHouseContainerName { get; set; }

    public string AccreditationContainerName { get; set; }
}