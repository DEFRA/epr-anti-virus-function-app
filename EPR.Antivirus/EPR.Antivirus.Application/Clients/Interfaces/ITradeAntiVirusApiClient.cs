namespace EPR.Antivirus.Application.Clients.Interfaces;

public interface ITradeAntivirusApiClient
{
    Task<Stream> GetFileAsync(string collection, Guid key);
}