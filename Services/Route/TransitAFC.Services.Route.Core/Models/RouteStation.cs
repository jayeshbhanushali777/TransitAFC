using System.ComponentModel.DataAnnotations;
using TransitAFC.Shared.Common.Models;

namespace TransitAFC.Services.Route.Core.Models
{
    public class RouteStation : BaseEntity
    {
        public Guid RouteId { get; set; }
        public Route Route { get; set; } = null!;

        public Guid StationId { get; set; }
        public Station Station { get; set; } = null!;

        public int StationOrder { get; set; } // Order in the route
        public decimal DistanceFromStart { get; set; } // Distance from route start
        public TimeSpan EstimatedTravelTime { get; set; } // Time from previous station
        public decimal FareFromStart { get; set; } // Cumulative fare from start

        public bool IsStopRequired { get; set; } = true; // For express routes
        public TimeSpan? DwellTime { get; set; } // How long vehicle stops

        [StringLength(500)]
        public string? Notes { get; set; }
    }
}