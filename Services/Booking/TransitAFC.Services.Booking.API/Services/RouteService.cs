using System.Text.Json;
using TransitAFC.Shared.Common.DTOs;

namespace TransitAFC.Services.Booking.API.Services
{
    public class RouteService : IRouteService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly ILogger<RouteService> _logger;

        public RouteService(HttpClient httpClient, IConfiguration configuration, ILogger<RouteService> logger)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<RouteServiceInfo?> GetRouteInfoAsync(Guid routeId, Guid sourceStationId, Guid destinationStationId)
        {
            try
            {
                var routeServiceUrl = _configuration["Services:RouteService:BaseUrl"];
                var response = await _httpClient.GetAsync($"{routeServiceUrl}/api/routes/{routeId}");

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("Route service returned {StatusCode} for route {RouteId}", response.StatusCode, routeId);
                    return null;
                }

                var content = await response.Content.ReadAsStringAsync();
                var apiResponse = JsonSerializer.Deserialize<ApiResponse<RouteDetailsResponse>>(content, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (apiResponse?.Data == null)
                    return null;

                var route = apiResponse.Data;

                // Find source and destination stations in route
                var sourceStation = route.Stations?.FirstOrDefault(s => s.Station.Id == sourceStationId);
                var destinationStation = route.Stations?.FirstOrDefault(s => s.Station.Id == destinationStationId);

                if (sourceStation == null || destinationStation == null)
                    return null;

                return new RouteServiceInfo
                {
                    RouteId = route.RouteId,
                    RouteCode = route.Code,
                    RouteName = route.Name,
                    SourceStationName = sourceStation.Station.Name,
                    DestinationStationName = destinationStation.Station.Name,
                    EstimatedDuration = route.EstimatedDuration,
                    BaseFare = Math.Abs(destinationStation.FareFromStart - sourceStation.FareFromStart),
                    Distance = Math.Abs(destinationStation.DistanceFromStart - sourceStation.DistanceFromStart)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting route info for route {RouteId}", routeId);
                return null;
            }
        }

        public async Task<bool> IsValidRouteAsync(Guid routeId, Guid sourceStationId, Guid destinationStationId)
        {
            var routeInfo = await GetRouteInfoAsync(routeId, sourceStationId, destinationStationId);
            return routeInfo != null;
        }
    }

    public class RouteDetailsResponse
    {
        public Guid RouteId { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public TimeSpan EstimatedDuration { get; set; }
        public decimal BaseFare { get; set; }
        public List<RouteStationInfo>? Stations { get; set; }
    }

    public class RouteStationInfo
    {
        public StationInfo Station { get; set; } = new();
        public decimal DistanceFromStart { get; set; }
        public decimal FareFromStart { get; set; }
    }

    public class StationInfo
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
    }
}