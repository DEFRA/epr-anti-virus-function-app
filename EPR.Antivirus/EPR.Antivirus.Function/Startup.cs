using System.Diagnostics.CodeAnalysis;
using EPR.Antivirus.Function;
using EPR.Antivirus.Function.Extensions;
using EPR.Common.Logging.Extensions;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;

[assembly: FunctionsStartup(typeof(Startup))]

namespace EPR.Antivirus.Function;

[ExcludeFromCodeCoverage]
public class Startup : FunctionsStartup
{
    public override void Configure(IFunctionsHostBuilder builder)
    {
        var services = builder.Services;
        services.ConfigureOptions();
        services.AddLogging();
        services.ConfigureLogging();
        services.AddApplicationInsightsTelemetry();
        services.AddAzureClients();
        services.AddHttpClients();
        services.AddServices();
    }
}
