namespace TransitAFC.Services.Booking.API.Services
{
    public class RouteServiceInfo
    {
        public Guid RouteId { get; set; }
        public string RouteCode { get; set; } = string.Empty;
        public string RouteName { get; set; } = string.Empty;
        public string SourceStationName { get; set; } = string.Empty;
        public string DestinationStationName { get; set; } = string.Empty;
        public TimeSpan EstimatedDuration { get; set; }
        public decimal BaseFare { get; set; }
        public decimal Distance { get; set; }
    }

    public interface IRouteService
    {
        Task<RouteServiceInfo?> GetRouteInfoAsync(Guid routeId, Guid sourceStationId, Guid destinationStationId);
        Task<bool> IsValidRouteAsync(Guid routeId, Guid sourceStationId, Guid destinationStationId);
    }
}