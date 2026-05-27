namespace EPR.Antivirus.Function;

using System.Diagnostics.CodeAnalysis;
using EPR.Antivirus.Function.Extensions;
using EPR.Common.Logging.Extensions;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.FeatureManagement;

[ExcludeFromCodeCoverage]
public static class Program
{
    public static void Main()
    {
        var host = new HostBuilder()
            .ConfigureFunctionsWorkerDefaults()
            .ConfigureServices(services =>
            {
                services.ConfigureOptions();
                services.AddLogging();
                services.ConfigureLogging();
                services.AddApplicationInsightsTelemetryWorkerService();
                services.ConfigureFunctionsApplicationInsights();
                services.AddAzureClients();
                services.AddHttpClients();
                services.AddServices();
                services.AddFeatureManagement();
            })
            .Build();

        host.Run();
    }
}
