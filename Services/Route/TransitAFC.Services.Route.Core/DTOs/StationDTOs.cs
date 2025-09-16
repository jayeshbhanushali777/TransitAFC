using System.ComponentModel.DataAnnotations;

namespace TransitAFC.Services.Route.Core.DTOs
{
    public class CreateStationRequest
    {
        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [StringLength(20)]
        public string Code { get; set; } = string.Empty;

        public string? Address { get; set; }

        [Required]
        [StringLength(100)]
        public string City { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string State { get; set; } = string.Empty;

        public string? PinCode { get; set; }

        [Required]
        [Range(-90, 90)]
        public double Latitude { get; set; }

        [Required]
        [Range(-180, 180)]
        public double Longitude { get; set; }

        [Required]
        public Guid TransportModeId { get; set; }

        public string? StationType { get; set; }
        public bool HasWheelchairAccess { get; set; }
        public bool HasParking { get; set; }
        public bool HasWiFi { get; set; }
        public bool HasRestroom { get; set; }
        public List<string> Amenities { get; set; } = new();
        public int PlatformCount { get; set; }
    }

    public class UpdateStationRequest
    {
        public string? Name { get; set; }
        public string? Address { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        public string? StationType { get; set; }
        public bool? HasWheelchairAccess { get; set; }
        public bool? HasParking { get; set; }
        public bool? HasWiFi { get; set; }
        public bool? HasRestroom { get; set; }
        public List<string>? Amenities { get; set; }
        public int? PlatformCount { get; set; }
        public bool? IsActive { get; set; }
    }

    public class StationResponse
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string State { get; set; } = string.Empty;
        public string? PinCode { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public string TransportMode { get; set; } = string.Empty;
        public string? StationType { get; set; }
        public bool HasWheelchairAccess { get; set; }
        public bool HasParking { get; set; }
        public bool HasWiFi { get; set; }
        public bool HasRestroom { get; set; }
        public List<string> Amenities { get; set; } = new();
        public int PlatformCount { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public List<string> ConnectedRoutes { get; set; } = new();
    }
}