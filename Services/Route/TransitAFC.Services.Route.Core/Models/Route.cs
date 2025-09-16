using System.ComponentModel.DataAnnotations;
using TransitAFC.Shared.Common.Models;

namespace TransitAFC.Services.Route.Core.Models
{
    public class Route : BaseEntity
    {
        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [StringLength(20)]
        public string Code { get; set; } = string.Empty; // Unique route code

        [StringLength(500)]
        public string? Description { get; set; }

        public Guid TransportModeId { get; set; }
        public TransportMode TransportMode { get; set; } = null!;

        public Guid StartStationId { get; set; }
        public Station StartStation { get; set; } = null!;

        public Guid EndStationId { get; set; }
        public Station EndStation { get; set; } = null!;

        public decimal TotalDistance { get; set; } // in kilometers
        public TimeSpan EstimatedDuration { get; set; }
        public decimal BaseFare { get; set; }

        [StringLength(20)]
        public string? RouteColor { get; set; } // For map display

        public bool IsActive { get; set; } = true;
        public bool IsExpress { get; set; } = false;

        public DateTime? ServiceStartTime { get; set; }
        public DateTime? ServiceEndTime { get; set; }

        public int FrequencyMinutes { get; set; } // Average frequency
        public int? MaxCapacity { get; set; }

        [StringLength(1000)]
        public string? RouteGeometry { get; set; } // GeoJSON string

        // Navigation properties
        public ICollection<RouteStation> RouteStations { get; set; } = new List<RouteStation>();
        public ICollection<Schedule> Schedules { get; set; } = new List<Schedule>();
    }
}