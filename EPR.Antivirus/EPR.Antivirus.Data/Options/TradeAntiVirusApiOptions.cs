namespace EPR.Antivirus.Data.Options;

using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

[ExcludeFromCodeCoverage]
public class TradeAntivirusApiOptions
{
    public const string Section = "TradeAntivirusApi";

    [Required]
    public string BaseUrl { get; set; }

    [Required]
    public string SubscriptionKey { get; set; }

    [Required]
    public string Scope { get; set; }

    [Required]
    public string TenantId { get; set; }

    [Required]
    public string ClientId { get; set; }

    [Required]
    public string ClientSecret { get; set; }

    [Required]
    public int Timeout { get; init; }

    public bool EnableDirectAccess { get; init; } = false;
}