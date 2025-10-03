using Newtonsoft.Json.Linq;
using System.Net.Http.Headers;
using System.Text.Json;
using TransitAFC.Shared.Common.DTOs;

namespace TransitAFC.Services.Payment.API.Services
{
    public class BookingService : IBookingService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly ILogger<BookingService> _logger;
        private readonly IHttpContextAccessor _httpContextAccessor; // ADD THIS

        public BookingService(
            HttpClient httpClient,
            IConfiguration configuration,
            ILogger<BookingService> logger,
            IHttpContextAccessor httpContextAccessor) // ADD THIS
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _logger = logger;
            _httpContextAccessor = httpContextAccessor; // ADD THIS
        }

        public async Task<BookingInfo?> GetBookingAsync(Guid bookingId, Guid userId)
        {
            try
            {
                var bookingServiceUrl = _configuration["Services:BookingService:BaseUrl"];

                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", GetCurrentToken());

                // Create the request to see what headers get added
                using var requestMessage = new HttpRequestMessage(HttpMethod.Get, $"{bookingServiceUrl}/api/bookings/{bookingId}");

                var response = await _httpClient.SendAsync(requestMessage);

                if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError("❌ Unauthorized response from booking service: {Error}", errorContent);
                    return null;
                }

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
                {
                    _logger.LogWarning("No booking data found in response for booking {BookingId}", bookingId);
                    return null;
                }

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
                _logger.LogError(ex, "❌ Error getting booking {BookingId}", bookingId);
                return null;
            }
        }

        private string? GetCurrentToken()
        {
            try
            {
                var context = _httpContextAccessor.HttpContext;
                if (context == null) return null;

                var authHeader = context.Request.Headers["Authorization"].FirstOrDefault();

                if (authHeader != null && authHeader.StartsWith("Bearer "))
                {
                    return authHeader.Substring("Bearer ".Length).Trim();
                }

                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting current token");
                return null;
            }
        }

        public async Task<bool> ConfirmBookingAsync(Guid bookingId, Guid userId, object confirmationData)
        {
            try
            {
                var bookingServiceUrl = _configuration["Services:BookingService:BaseUrl"];
                var content = new StringContent(JsonSerializer.Serialize(confirmationData), System.Text.Encoding.UTF8, "application/json");
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", GetCurrentToken());

                _logger.LogInformation("Confirming booking {BookingId} for user {UserId}", bookingId, userId);

                var response = await _httpClient.PostAsync($"{bookingServiceUrl}/api/bookings/{bookingId}/confirm", content);

                if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError("❌ Unauthorized response when confirming booking {BookingId}: {Error}", bookingId, errorContent);
                    return false;
                }

                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error confirming booking {BookingId}", bookingId);
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