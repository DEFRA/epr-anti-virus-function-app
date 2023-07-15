namespace EPR.Antivirus.Function.Extensions;

using System.Diagnostics.CodeAnalysis;
using System.Net.Http.Headers;
using Application.Clients;
using Application.Clients.Interfaces;
using Application.Extensions;
using Application.Handlers;
using Application.Services;
using Application.Services.Interfaces;
using Data.Options;
using Microsoft.Extensions.Azure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

[ExcludeFromCodeCoverage]
public static class ConfigurationExtensions
{
    public static void ConfigureOptions(this IServiceCollection services)
    {
        services.ConfigureSection<SubmissionStatusApiOptions>(SubmissionStatusApiOptions.Section);
        services.ConfigureSection<ServiceBusOptions>(ServiceBusOptions.Section);
        services.ConfigureSection<BlobStorageOptions>(BlobStorageOptions.Section);
        services.ConfigureSection<TradeAntivirusApiOptions>(TradeAntivirusApiOptions.Section);
    }

    public static void AddAzureClients(this IServiceCollection services)
    {
        var sp = services.BuildServiceProvider();

        var serviceBusOptions = sp.GetRequiredService<IOptions<ServiceBusOptions>>();
        var blobStorageOptions = sp.GetRequiredService<IOptions<BlobStorageOptions>>();

        services.AddAzureClients(cb =>
        {
            cb.AddServiceBusClient(serviceBusOptions.Value.ConnectionString);
            cb.AddBlobServiceClient(blobStorageOptions.Value.ConnectionString);
        });
    }

    public static void AddHttpClients(this IServiceCollection services)
    {
        services.AddHttpClient<ISubmissionStatusApiClient, SubmissionStatusApiClient>((sp, c) =>
        {
            var submissionStatusApiOptions = sp.GetRequiredService<IOptions<SubmissionStatusApiOptions>>().Value;
            c.BaseAddress = new Uri($"{submissionStatusApiOptions.BaseUrl}/v1/");
            c.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        });

        var tradeAntivirusOptions = services.BuildServiceProvider().GetRequiredService<IOptions<TradeAntivirusApiOptions>>().Value;

        if (tradeAntivirusOptions.EnableDirectAccess)
        {
            services.AddHttpClient<ITradeAntivirusApiClient, TradeAntivirusApiClient>(client =>
            {
                client.BaseAddress = new Uri($"{tradeAntivirusOptions.BaseUrl}/");
                client.Timeout = TimeSpan.FromSeconds(tradeAntivirusOptions.Timeout);
            });
        }
        else
        {
            services.AddHttpClient<ITradeAntivirusApiClient, TradeAntivirusApiClient>(client =>
            {
                client.BaseAddress = new Uri($"{tradeAntivirusOptions.BaseUrl}/v1/");
                client.Timeout = TimeSpan.FromSeconds(tradeAntivirusOptions.Timeout);
                client.DefaultRequestHeaders.Add("OCP-APIM-Subscription-Key", tradeAntivirusOptions.SubscriptionKey);
            }).AddHttpMessageHandler<TradeAntivirusApiAuthorizationHandler>();
        }
    }

    public static void AddServices(this IServiceCollection services)
    {
        services.AddScoped<IAntivirusService, AntivirusService>();
        services.AddScoped<IServiceBusQueueClient, ServiceBusQueueClient>();
        services.AddScoped<IBlobStorageService, BlobStorageService>();
        services.AddTransient<TradeAntivirusApiAuthorizationHandler>();
    }
}