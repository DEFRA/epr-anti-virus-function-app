namespace EPR.Antivirus.Function;

using System.Text.Json;
using System.Threading.Tasks;
using Application.Extensions;
using Application.Services.Interfaces;
using Data.DTOs.TradeAntivirusQueue;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;

public class ServiceBusQueueTrigger
{
    private readonly IAntivirusService _antivirusService;
    private readonly ILogger<ServiceBusQueueTrigger> _logger;

    public ServiceBusQueueTrigger(IAntivirusService antivirusService, ILogger<ServiceBusQueueTrigger> logger)
    {
        _antivirusService = antivirusService;
        _logger = logger;
    }

    [FunctionName("ServiceBusQueueTrigger")]
    public async Task RunAsync(
        [ServiceBusTrigger("%TradeAntivirusServiceBus:AntivirusTopicName%", "%TradeAntivirusServiceBus:SubscriptionName%", Connection = "TradeAntivirusServiceBus:ConnectionString")]
        string message)
    {
        _logger.LogEnter();

        var messageObj = JsonSerializer.Deserialize<TradeAntivirusQueueResult>(message);
        await _antivirusService.HandleAsync(messageObj);

        _logger.LogExit();
    }
}