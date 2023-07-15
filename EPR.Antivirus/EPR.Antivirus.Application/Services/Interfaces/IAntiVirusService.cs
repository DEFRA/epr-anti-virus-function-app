namespace EPR.Antivirus.Application.Services.Interfaces;

using Data.DTOs.TradeAntivirusQueue;

public interface IAntivirusService
{
    Task HandleAsync(TradeAntivirusQueueResult message);
}