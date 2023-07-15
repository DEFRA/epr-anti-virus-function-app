namespace EPR.Antivirus.Data.Options;

using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

[ExcludeFromCodeCoverage]
public class TradeAntivirusApiOptions
{
    public const string Section = "TradeAntivirusApi";

    [Required]
    public string BaseUrl { get; init; }

    [Required]
    public string SubscriptionKey { get; init; }

    [Required]
    public string Scope { get; init; }

    [Required]
    public string TenantId { get; init; }

    [Required]
    public string ClientId { get; init; }

    [Required]
    public string ClientSecret { get; init; }

    [Required]
    public int Timeout { get; init; }

    public bool EnableDirectAccess { get; init; } = false;
}