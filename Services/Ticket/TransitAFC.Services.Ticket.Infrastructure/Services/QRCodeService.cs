using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using QRCoder;
using System.Security.Cryptography;
using System.Text;
using TransitAFC.Services.Ticket.Core.Models;
using TransitAFC.Services.Ticket.Infrastructure.Repositories;

namespace TransitAFC.Services.Ticket.Infrastructure.Services
{
    public class QRCodeService : IQRCodeService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<QRCodeService> _logger;
        private readonly ITicketRepository _ticketRepository;
        private readonly string _encryptionKey;
        private readonly string _hashSalt;

        public QRCodeService(IConfiguration configuration, ILogger<QRCodeService> logger, ITicketRepository ticketRepository)
        {
            _configuration = configuration;
            _logger = logger;
            _ticketRepository = ticketRepository;
            _encryptionKey = _configuration["QRCode:EncryptionKey"] ?? "DefaultQRCodeEncryptionKey123456789012";
            _hashSalt = _configuration["QRCode:HashSalt"] ?? "DefaultQRCodeHashSalt123456789012";
        }

        public async Task<(string qrData, byte[] qrImage, string hash)> GenerateQRCodeAsync(Core.Models.Ticket ticket)
        {
            try
            {
                _logger.LogInformation("Generating QR code for ticket {TicketNumber}", ticket.TicketNumber);

                // Create QR data payload
                var qrPayload = new
                {
                    TicketId = ticket.Id,
                    TicketNumber = ticket.TicketNumber,
                    UserId = ticket.UserId,
                    SourceStation = ticket.SourceStationCode,
                    DestinationStation = ticket.DestinationStationCode,
                    ValidFrom = ticket.ValidFrom,
                    ValidUntil = ticket.ValidUntil,
                    Type = ticket.Type.ToString(),
                    Status = ticket.Status.ToString(),
                    PassengerName = ticket.PassengerName,
                    FinalPrice = ticket.FinalPrice,
                    Currency = ticket.Currency,
                    MaxUsage = ticket.MaxUsageCount,
                    AllowsTransfer = ticket.AllowsTransfer,
                    GeneratedAt = DateTime.UtcNow,
                    Signature = GenerateSignature(ticket)
                };

                var jsonData = JsonConvert.SerializeObject(qrPayload);
                var encryptedData = await EncryptQRDataAsync(jsonData, _encryptionKey);
                var hash = await GenerateQRHashAsync(encryptedData);
                var qrImage = await GenerateQRCodeImageAsync(encryptedData);

                _logger.LogInformation("QR code generated successfully for ticket {TicketNumber}", ticket.TicketNumber);

                return (encryptedData, qrImage, hash);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating QR code for ticket {TicketNumber}", ticket.TicketNumber);
                throw;
            }
        }

        public async Task<byte[]> GenerateQRCodeImageAsync(string data, int size = 300, string format = "PNG")
        {
            try
            {
                using var qrGenerator = new QRCodeGenerator();
                using var qrCodeData = qrGenerator.CreateQrCode(data, QRCodeGenerator.ECCLevel.M);

                if (format.ToUpper() == "SVG")
                {
                    using var qrCode = new SvgQRCode(qrCodeData);
                    var svgString = qrCode.GetGraphic(20);
                    return Encoding.UTF8.GetBytes(svgString);
                }
                else
                {
                    using var qrCode = new PngByteQRCode(qrCodeData);
                    return qrCode.GetGraphic(20);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating QR code image");
                throw;
            }
        }

        public async Task<bool> ValidateQRCodeAsync(string qrData, string hash)
        {
            try
            {
                var generatedHash = await GenerateQRHashAsync(qrData);
                return generatedHash == hash;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating QR code");
                return false;
            }
        }

        public async Task<string> EncryptQRDataAsync(string data, string key)
        {
            try
            {
                using var aes = Aes.Create();
                aes.Key = Encoding.UTF8.GetBytes(key.PadRight(32).Substring(0, 32));
                aes.IV = new byte[16]; // Use zero IV for simplicity - in production, use random IV

                using var encryptor = aes.CreateEncryptor();
                var dataBytes = Encoding.UTF8.GetBytes(data);
                var encryptedBytes = encryptor.TransformFinalBlock(dataBytes, 0, dataBytes.Length);

                return Convert.ToBase64String(encryptedBytes);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error encrypting QR data");
                throw;
            }
        }

        public async Task<string> DecryptQRDataAsync(string encryptedData, string key)
        {
            try
            {
                using var aes = Aes.Create();
                aes.Key = Encoding.UTF8.GetBytes(key.PadRight(32).Substring(0, 32));
                aes.IV = new byte[16]; // Use zero IV for simplicity - in production, use random IV

                using var decryptor = aes.CreateDecryptor();
                var encryptedBytes = Convert.FromBase64String(encryptedData);
                var decryptedBytes = decryptor.TransformFinalBlock(encryptedBytes, 0, encryptedBytes.Length);

                return Encoding.UTF8.GetString(decryptedBytes);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error decrypting QR data");
                throw;
            }
        }

        public async Task<string> GenerateQRHashAsync(string data)
        {
            try
            {
                using var sha256 = SHA256.Create();
                var saltedData = data + _hashSalt;
                var hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(saltedData));
                return Convert.ToBase64String(hashBytes);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating QR hash");
                throw;
            }
        }

        public async Task<TicketQRCode> RegenerateQRCodeAsync(Guid ticketId, string reason)
        {
            try
            {
                var ticket = await _ticketRepository.GetByIdAsync(ticketId);
                if (ticket == null)
                {
                    throw new InvalidOperationException("Ticket not found");
                }

                // Mark current QR codes as expired
                foreach (var qrCode in ticket.QRCodes.Where(q => q.Status == QRCodeStatus.Active))
                {
                    qrCode.Status = QRCodeStatus.Expired;
                    qrCode.InvalidatedAt = DateTime.UtcNow;
                }

                // Generate new QR code
                var (qrData, qrImage, hash) = await GenerateQRCodeAsync(ticket);

                var newQRCode = new TicketQRCode
                {
                    TicketId = ticket.Id,
                    QRCodeId = GenerateQRCodeId(),
                    QRCodeData = qrData,
                    QRCodeHash = hash,
                    QRCodeImage = qrImage,
                    Status = QRCodeStatus.Active,
                    GeneratedAt = DateTime.UtcNow,
                    ExpiresAt = ticket.ValidUntil,
                    EncryptionKey = _encryptionKey,
                    IsRegenerated = true,
                    RegenerationReason = reason
                };

                ticket.QRCodes.Add(newQRCode);
                ticket.QRCodeData = qrData;
                ticket.QRCodeHash = hash;
                ticket.QRCodeImage = qrImage;

                await _ticketRepository.UpdateAsync(ticket);

                return newQRCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error regenerating QR code for ticket {TicketId}", ticketId);
                throw;
            }
        }

        private string GenerateSignature(Core.Models.Ticket ticket)
        {
            var signatureData = $"{ticket.Id}{ticket.TicketNumber}{ticket.UserId}{ticket.FinalPrice}{ticket.ValidFrom:yyyyMMdd}{ticket.ValidUntil:yyyyMMdd}";
            using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(_encryptionKey));
            var signatureBytes = hmac.ComputeHash(Encoding.UTF8.GetBytes(signatureData));
            return Convert.ToBase64String(signatureBytes);
        }

        private string GenerateQRCodeId()
        {
            return $"QR{DateTime.UtcNow:yyyyMMddHHmmss}{Random.Shared.Next(1000, 9999)}";
        }
    }
}