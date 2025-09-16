using System.ComponentModel.DataAnnotations;

namespace TransitAFC.Services.Route.Core.DTOs
{
    public class RouteSearchRequest
    {
        [Required]
        public string Source { get; set; } = string.Empty; // Station code or name

        [Required]
        public string Destination { get; set; } = string.Empty; // Station code or name

        public DateTime? DepartureTime { get; set; }
        public string? TransportMode { get; set; } // BUS, MET, TRN, ALL
        public bool IncludeAccessibility { get; set; } = false;
        public bool PreferFastest { get; set; } = true; // vs cheapest
        public int MaxTransfers { get; set; } = 3;
        public string? Language { get; set; } = "en";
    }

    public class RouteSearchResponse
    {
        public List<RouteOption> Routes { get; set; } = new();
        public StationInfo? SourceStation { get; set; }
        public StationInfo? DestinationStation { get; set; }
        public DateTime SearchTime { get; set; }
        public string SearchId { get; set; } = string.Empty;
    }

    public class RouteOption
    {
        public string RouteId { get; set; } = string.Empty;
        public List<RouteSegment> Segments { get; set; } = new();
        public decimal TotalFare { get; set; }
        public TimeSpan TotalDuration { get; set; }
        public decimal TotalDistance { get; set; }
        public int TransferCount { get; set; }
        public DateTime DepartureTime { get; set; }
        public DateTime ArrivalTime { get; set; }
        public string RouteType { get; set; } = string.Empty; // Direct, Transfer
        public bool IsAccessible { get; set; }
        public int ComfortScore { get; set; } // 1-5 rating
        public string? Polyline { get; set; } // For map display
    }

    public class RouteSegment
    {
        public Guid RouteId { get; set; }
        public string RouteName { get; set; } = string.Empty;
        public string RouteCode { get; set; } = string.Empty;
        public string TransportMode { get; set; } = string.Empty;
        public StationInfo StartStation { get; set; } = new();
        public StationInfo EndStation { get; set; } = new();
        public DateTime DepartureTime { get; set; }
        public DateTime ArrivalTime { get; set; }
        public TimeSpan Duration { get; set; }
        public decimal Distance { get; set; }
        public decimal Fare { get; set; }
        public string? VehicleNumber { get; set; }
        public List<StationInfo> IntermediateStations { get; set; } = new();
        public string? Color { get; set; }
        public bool IsWalkingSegment { get; set; } = false;
    }

    public class StationInfo
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public string TransportMode { get; set; } = string.Empty;
        public List<string> Amenities { get; set; } = new();
        public bool IsAccessible { get; set; }
    }

    public class SmartRouteRequest
    {
        [Required]
        public string Source { get; set; } = string.Empty;

        [Required]
        public string Destination { get; set; } = string.Empty;

        public DateTime? PreferredDepartureTime { get; set; }
        public string? UserPreferences { get; set; } // JSON string
        public string? WeatherCondition { get; set; }
        public string? CrowdPreference { get; set; } // Low, Medium, High
        public bool UseAI { get; set; } = true;
        public string? UserId { get; set; } // For personalization
    }

    public class NearbyStationsRequest
    {
        [Required]
        [Range(-90, 90)]
        public double Latitude { get; set; }

        [Required]
        [Range(-180, 180)]
        public double Longitude { get; set; }

        [Range(0.1, 50)]
        public double RadiusKm { get; set; } = 2.0;

        public string? TransportMode { get; set; }
        public int MaxResults { get; set; } = 10;
    }

    public class RouteDetailsRequest
    {
        [Required]
        public string RouteCode { get; set; } = string.Empty;
        public DateTime? Date { get; set; }
        public bool IncludeSchedule { get; set; } = false;
        public bool IncludeRealTime { get; set; } = false;
    }

    public class RouteDetailsResponse
    {
        public Guid RouteId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string TransportMode { get; set; } = string.Empty;
        public StationInfo StartStation { get; set; } = new();
        public StationInfo EndStation { get; set; } = new();
        public decimal TotalDistance { get; set; }
        public TimeSpan EstimatedDuration { get; set; }
        public decimal BaseFare { get; set; }
        public List<RouteStationInfo> Stations { get; set; } = new();
        public List<ScheduleInfo> Schedules { get; set; } = new();
        public string? RouteColor { get; set; }
        public bool IsActive { get; set; }
        public RealTimeInfo? RealTime { get; set; }
    }

    public class RouteStationInfo
    {
        public StationInfo Station { get; set; } = new();
        public int Order { get; set; }
        public decimal DistanceFromStart { get; set; }
        public TimeSpan TravelTimeFromPrevious { get; set; }
        public decimal FareFromStart { get; set; }
        public bool IsStopRequired { get; set; }
    }

    public class ScheduleInfo
    {
        public TimeSpan DepartureTime { get; set; }
        public TimeSpan? ArrivalTime { get; set; }
        public string DayOfWeek { get; set; } = string.Empty;
        public string? VehicleNumber { get; set; }
        public bool IsOperational { get; set; }
    }

    public class RealTimeInfo
    {
        public List<VehicleLocation> Vehicles { get; set; } = new();
        public List<ServiceAlert> Alerts { get; set; } = new();
        public DateTime LastUpdated { get; set; }
    }

    public class VehicleLocation
    {
        public string VehicleNumber { get; set; } = string.Empty;
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public string Status { get; set; } = string.Empty; // On Time, Delayed, etc.
        public TimeSpan? Delay { get; set; }
        public string? NextStation { get; set; }
        public DateTime LastUpdated { get; set; }
    }

    public class ServiceAlert
    {
        public string AlertId { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Severity { get; set; } = string.Empty; // Info, Warning, Critical
        public DateTime StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public List<string> AffectedRoutes { get; set; } = new();
    }

    public class CreateRouteRequest
    {
        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [StringLength(20)]
        public string Code { get; set; } = string.Empty;

        public string? Description { get; set; }

        [Required]
        public Guid TransportModeId { get; set; }

        [Required]
        public Guid StartStationId { get; set; }

        [Required]
        public Guid EndStationId { get; set; }

        public decimal TotalDistance { get; set; }
        public TimeSpan EstimatedDuration { get; set; }
        public decimal BaseFare { get; set; }
        public string? RouteColor { get; set; }
        public bool IsExpress { get; set; } = false;
        public int FrequencyMinutes { get; set; }
        public List<CreateRouteStationRequest> Stations { get; set; } = new();
    }

    public class CreateRouteStationRequest
    {
        [Required]
        public Guid StationId { get; set; }
        public int StationOrder { get; set; }
        public decimal DistanceFromStart { get; set; }
        public TimeSpan EstimatedTravelTime { get; set; }
        public decimal FareFromStart { get; set; }
        public bool IsStopRequired { get; set; } = true;
    }

    public class UpdateRouteRequest
    {
        public string? Name { get; set; }
        public string? Description { get; set; }
        public decimal? TotalDistance { get; set; }
        public TimeSpan? EstimatedDuration { get; set; }
        public decimal? BaseFare { get; set; }
        public string? RouteColor { get; set; }
        public bool? IsActive { get; set; }
        public bool? IsExpress { get; set; }
        public int? FrequencyMinutes { get; set; }
    }
}