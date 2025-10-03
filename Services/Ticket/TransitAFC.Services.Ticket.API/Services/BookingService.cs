using System.Net.Http.Headers;
using System.Text.Json;
using TransitAFC.Shared.Common.DTOs;

namespace TransitAFC.Services.Ticket.API.Services
{
    public class BookingService : IBookingService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly ILogger<BookingService> _logger;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public BookingService(HttpClient httpClient, IConfiguration configuration, ILogger<BookingService> logger, IHttpContextAccessor httpContextAccessor)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _logger = logger;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<BookingInfo?> GetBookingAsync(Guid bookingId, Guid userId)
        {
            try
            {
                var bookingServiceUrl = _configuration["Services:BookingService:BaseUrl"];
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", GetCurrentToken());
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
                    Status = booking.Status?.ToString() ?? "Unknown",
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
    }

    public class BookingResponse
    {
        public Guid Id { get; set; }
        public string BookingNumber { get; set; } = string.Empty;
        public object? Status { get; set; }
        public decimal FinalAmount { get; set; }
        public Guid UserId { get; set; }
    }
}