using System.ComponentModel.DataAnnotations;
using TransitAFC.Shared.Common.Models;

namespace TransitAFC.Services.Booking.Core.Models
{
    public class BookingHistory : BaseEntity
    {
        [Required]
        public Guid BookingId { get; set; }
        public Booking Booking { get; set; } = null!;

        [Required]
        public BookingStatus FromStatus { get; set; }

        [Required]
        public BookingStatus ToStatus { get; set; }

        [Required]
        [StringLength(100)]
        public string Action { get; set; } = string.Empty; // Created, Confirmed, Cancelled, etc.

        [StringLength(500)]
        public string? Reason { get; set; }

        [StringLength(1000)]
        public string? Notes { get; set; }

        [Required]
        public Guid ActionBy { get; set; } // User or system ID

        [StringLength(100)]
        public string ActionByType { get; set; } = "User"; // User, System, Admin

        [StringLength(50)]
        public string? IpAddress { get; set; }

        [StringLength(100)]
        public string? UserAgent { get; set; }

        public DateTime ActionTime { get; set; } = DateTime.UtcNow;
    }
}