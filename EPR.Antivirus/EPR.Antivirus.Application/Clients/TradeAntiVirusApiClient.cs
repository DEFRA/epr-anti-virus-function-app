namespace EPR.Antivirus.Application.Clients;

using Exceptions;
using Interfaces;
using Microsoft.Extensions.Logging;

public class TradeAntivirusApiClient : ITradeAntivirusApiClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<TradeAntivirusApiClient> _logger;

    public TradeAntivirusApiClient(
        HttpClient httpClient, ILogger<TradeAntivirusApiClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<Stream> GetFileAsync(string collection, Guid key)
    {
        try
        {
            var response = await _httpClient.GetAsync($"files/stream/{collection}/{key}");
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadAsStreamAsync();
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "A success status code was not received when requesting antivirus file");
            throw new TradeAntivirusApiClientException(
                "A success status code was not received when requesting antivirus file", exception);
        }
    }
}