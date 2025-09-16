using System.ComponentModel.DataAnnotations;
using TransitAFC.Shared.Common.Models;

namespace TransitAFC.Services.Ticket.Core.Models
{
    public class TicketHistory : BaseEntity
    {
        [Required]
        public Guid TicketId { get; set; }
        public Ticket Ticket { get; set; } = null!;

        [Required]
        public TicketStatus FromStatus { get; set; }

        [Required]
        public TicketStatus ToStatus { get; set; }

        [Required]
        [StringLength(100)]
        public string Action { get; set; } = string.Empty; // Generated, Activated, Used, Cancelled, etc.

        [StringLength(500)]
        public string? Reason { get; set; }

        [StringLength(1000)]
        public string? Notes { get; set; }

        [Required]
        public Guid ActionBy { get; set; } // User or system ID

        [StringLength(100)]
        public string ActionByType { get; set; } = "System"; // User, System, Admin, Operator

        [StringLength(100)]
        public string? ActionByName { get; set; }

        [StringLength(50)]
        public string? IpAddress { get; set; }

        [StringLength(200)]
        public string? UserAgent { get; set; }

        public DateTime ActionTime { get; set; } = DateTime.UtcNow;

        // Location Information
        public Guid? StationId { get; set; }

        [StringLength(100)]
        public string? StationName { get; set; }

        public Guid? DeviceId { get; set; }

        [StringLength(50)]
        public string? DeviceName { get; set; }

        // Change Details
        [StringLength(2000)]
        public string? ChangeDetails { get; set; } // JSON string of what changed

        [StringLength(100)]
        public string? ReferenceId { get; set; } // Related transaction/validation ID

        [StringLength(2000)]
        public string? AdditionalData { get; set; } // JSON string for extra information
    }
}