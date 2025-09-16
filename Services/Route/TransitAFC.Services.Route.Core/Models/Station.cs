using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TransitAFC.Shared.Common.Models;
using NetTopologySuite.Geometries;

namespace TransitAFC.Services.Route.Core.Models
{
    public class Station : BaseEntity
    {
        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [StringLength(20)]
        public string Code { get; set; } = string.Empty; // Unique station code

        [StringLength(200)]
        public string? Address { get; set; }

        [StringLength(100)]
        public string City { get; set; } = string.Empty;

        [StringLength(100)]
        public string State { get; set; } = string.Empty;

        [StringLength(10)]
        public string? PinCode { get; set; }

        // Geographic coordinates using NetTopologySuite
        [Column(TypeName = "geography")]
        public Point Location { get; set; } = null!;

        public double Latitude { get; set; }
        public double Longitude { get; set; }

        public Guid TransportModeId { get; set; }
        public TransportMode TransportMode { get; set; } = null!;

        [StringLength(50)]
        public string? StationType { get; set; } // Terminal, Junction, Regular

        public bool HasWheelchairAccess { get; set; }
        public bool HasParking { get; set; }
        public bool HasWiFi { get; set; }
        public bool HasRestroom { get; set; }

        [StringLength(500)]
        public string? Amenities { get; set; } // JSON string of amenities

        public int PlatformCount { get; set; }
        public DateTime? LastUpdated { get; set; }

        // Navigation properties
        public ICollection<RouteStation> RouteStations { get; set; } = new List<RouteStation>();
        public ICollection<Schedule> Schedules { get; set; } = new List<Schedule>();
    }
}