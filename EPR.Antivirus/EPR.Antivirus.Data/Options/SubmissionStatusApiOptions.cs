namespace EPR.Antivirus.Data.Options;

using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

[ExcludeFromCodeCoverage]
public class SubmissionStatusApiOptions
{
    public const string Section = "SubmissionApi";

    [Required]
    public string BaseUrl { get; init; }
}