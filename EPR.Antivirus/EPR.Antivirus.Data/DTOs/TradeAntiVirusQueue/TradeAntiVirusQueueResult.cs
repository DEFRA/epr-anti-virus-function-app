namespace EPR.Antivirus.Data.DTOs.TradeAntivirusQueue;

using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;
using Enums;

[ExcludeFromCodeCoverage]
public class TradeAntivirusQueueResult
{
    public Guid Key { get; set; }

    public string Collection { get; set; }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public ScanResult Status { get; set; }
}