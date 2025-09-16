using System.Text.Json;
using TransitAFC.Shared.Common.DTOs;

namespace TransitAFC.Services.Payment.API.Services
{
    public class BookingService : IBookingService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly ILogger<BookingService> _logger;

        public BookingService(HttpClient httpClient, IConfiguration configuration, ILogger<BookingService> logger)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<BookingInfo?> GetBookingAsync(Guid bookingId, Guid userId)
        {
            try
            {
                var bookingServiceUrl = _configuration["Services:BookingService:BaseUrl"];
                var response = await _httpClient.GetAsync($"{bookingServiceUrl}/api/bookings/{bookingId}");

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("Booking service returned {StatusCode} for booking {BookingId}", response.StatusCode, bookingId);
                    return null;
                }

                var content = await response.Content.ReadAsStringAsync();
                var apiResponse = JsonSerializer.Deserialize<ApiResponse<BookingResponse>>(content, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (apiResponse?.Data == null)
                    return null;

                var booking = apiResponse.Data;

                return new BookingInfo
                {
                    Id = booking.Id,
                    BookingNumber = booking.BookingNumber,
                    Status = booking.Status.ToString(),
                    Amount = booking.FinalAmount,
                    UserId = booking.UserId
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting booking {BookingId}", bookingId);
                return null;
            }
        }

        public async Task<bool> ConfirmBookingAsync(Guid bookingId, Guid userId, object confirmationData)
        {
            try
            {
                var bookingServiceUrl = _configuration["Services:BookingService:BaseUrl"];
                var content = new StringContent(JsonSerializer.Serialize(confirmationData), System.Text.Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync($"{bookingServiceUrl}/api/bookings/{bookingId}/confirm", content);

                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error confirming booking {BookingId}", bookingId);
                return false;
            }
        }
    }

    public class BookingResponse
    {
        public Guid Id { get; set; }
        public string BookingNumber { get; set; } = string.Empty;
        public object Status { get; set; } = string.Empty;
        public decimal FinalAmount { get; set; }
        public Guid UserId { get; set; }
    }
}