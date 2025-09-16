using System.ComponentModel.DataAnnotations;
using TransitAFC.Shared.Common.Models;

namespace TransitAFC.Services.Route.Core.Models
{
    public class Schedule : BaseEntity
    {
        public Guid RouteId { get; set; }
        public Route Route { get; set; } = null!;

        public Guid StationId { get; set; }
        public Station Station { get; set; } = null!;

        public TimeSpan DepartureTime { get; set; }
        public TimeSpan? ArrivalTime { get; set; }

        [StringLength(20)]
        public string DayOfWeek { get; set; } = string.Empty; // Monday, Tuesday, etc.

        public bool IsWeekday { get; set; }
        public bool IsWeekend { get; set; }
        public bool IsHoliday { get; set; }

        [StringLength(50)]
        public string? VehicleNumber { get; set; }
        public int? VehicleCapacity { get; set; }

        public DateTime EffectiveFrom { get; set; }
        public DateTime? EffectiveTo { get; set; }

        [StringLength(500)]
        public string? Notes { get; set; }
    }
}