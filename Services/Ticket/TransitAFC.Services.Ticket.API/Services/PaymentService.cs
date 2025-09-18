using System.Text.Json;
using TransitAFC.Shared.Common.DTOs;

namespace TransitAFC.Services.Ticket.API.Services
{
    public class PaymentInfo
    {
        public Guid Id { get; set; }
        public string PaymentId { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public Guid UserId { get; set; }
        public Guid BookingId { get; set; }
    }

    public class PaymentService : IPaymentService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly ILogger<PaymentService> _logger;

        public PaymentService(HttpClient httpClient, IConfiguration configuration, ILogger<PaymentService> logger)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<PaymentInfo?> GetPaymentByBookingIdAsync(Guid bookingId, Guid userId)
        {
            try
            {
                var paymentServiceUrl = _configuration["Services:PaymentService:BaseUrl"];
                var response = await _httpClient.GetAsync($"{paymentServiceUrl}/api/payments/booking/{bookingId}");

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("Payment service returned {StatusCode} for booking {BookingId}", response.StatusCode, bookingId);
                    return null;
                }

                var content = await response.Content.ReadAsStringAsync();
                var apiResponse = JsonSerializer.Deserialize<ApiResponse<PaymentResponse>>(content, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (apiResponse?.Data == null)
                    return null;

                var payment = apiResponse.Data;

                return new PaymentInfo
                {
                    Id = payment.Id,
                    PaymentId = payment.PaymentId,
                    Status = payment.Status?.ToString() ?? "Unknown",
                    Amount = payment.TotalAmount,
                    UserId = payment.UserId,
                    BookingId = payment.BookingId
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting payment for booking {BookingId}", bookingId);
                return null;
            }
        }

        public async Task<bool> ProcessRefundAsync(Guid paymentId, Guid userId, object refundRequest)
        {
            try
            {
                var paymentServiceUrl = _configuration["Services:PaymentService:BaseUrl"];
                var content = new StringContent(JsonSerializer.Serialize(refundRequest), System.Text.Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync($"{paymentServiceUrl}/api/payments/{paymentId}/refund", content);

                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing refund for payment {PaymentId}", paymentId);
                return false;
            }
        }
    }

    public class PaymentResponse
    {
        public Guid Id { get; set; }
        public string PaymentId { get; set; } = string.Empty;
        public object? Status { get; set; }
        public decimal TotalAmount { get; set; }
        public Guid UserId { get; set; }
        public Guid BookingId { get; set; }
    }
}