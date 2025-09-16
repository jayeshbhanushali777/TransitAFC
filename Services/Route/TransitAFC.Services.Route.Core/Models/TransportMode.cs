using System.ComponentModel.DataAnnotations;
using TransitAFC.Shared.Common.Models;
using static System.Collections.Specialized.BitVector32;

namespace TransitAFC.Services.Route.Core.Models
{
    public class TransportMode : BaseEntity
    {
        [Required]
        [StringLength(50)]
        public string Name { get; set; } = string.Empty; // Bus, Metro, Train, etc.

        [StringLength(10)]
        public string Code { get; set; } = string.Empty; // BUS, MET, TRN

        [StringLength(500)]
        public string? Description { get; set; }

        [StringLength(50)]
        public string? IconUrl { get; set; }

        [StringLength(20)]
        public string? Color { get; set; } // For UI display

        public decimal BaseFare { get; set; }
        public decimal FarePerKm { get; set; }
        public int MaxCapacity { get; set; }
        public bool IsRealTimeEnabled { get; set; }

        // Navigation properties
        public ICollection<Route> Routes { get; set; } = new List<Route>();
        public ICollection<Station> Stations { get; set; } = new List<Station>();
    }
}