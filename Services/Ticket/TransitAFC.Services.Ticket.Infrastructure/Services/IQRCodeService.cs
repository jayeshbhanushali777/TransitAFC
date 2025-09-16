using TransitAFC.Services.Ticket.Core.Models;

namespace TransitAFC.Services.Ticket.Infrastructure.Services
{
    public interface IQRCodeService
    {
        Task<(string qrData, byte[] qrImage, string hash)> GenerateQRCodeAsync(Core.Models.Ticket ticket);
        Task<byte[]> GenerateQRCodeImageAsync(string data, int size = 300, string format = "PNG");
        Task<bool> ValidateQRCodeAsync(string qrData, string hash);
        Task<string> EncryptQRDataAsync(string data, string key);
        Task<string> DecryptQRDataAsync(string encryptedData, string key);
        Task<string> GenerateQRHashAsync(string data);
        Task<TicketQRCode> RegenerateQRCodeAsync(Guid ticketId, string reason);
    }
}