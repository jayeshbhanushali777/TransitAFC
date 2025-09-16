using System.ComponentModel.DataAnnotations;
using TransitAFC.Shared.Common.Models;

namespace TransitAFC.Services.Payment.Core.Models
{
    public class PaymentHistory : BaseEntity
    {
        [Required]
        public Guid PaymentId { get; set; }
        public Payment Payment { get; set; } = null!;

        [Required]
        public PaymentStatus FromStatus { get; set; }

        [Required]
        public PaymentStatus ToStatus { get; set; }

        [Required]
        [StringLength(100)]
        public string Action { get; set; } = string.Empty; // Created, Processed, Failed, etc.

        [StringLength(500)]
        public string? Reason { get; set; }

        [StringLength(1000)]
        public string? Notes { get; set; }

        [Required]
        public Guid ActionBy { get; set; } // User or system ID

        [StringLength(100)]
        public string ActionByType { get; set; } = "System"; // User, System, Admin, Gateway

        [StringLength(50)]
        public string? IpAddress { get; set; }

        [StringLength(200)]
        public string? UserAgent { get; set; }

        public DateTime ActionTime { get; set; } = DateTime.UtcNow;

        [StringLength(100)]
        public string? GatewayReference { get; set; }

        [StringLength(2000)]
        public string? AdditionalData { get; set; } // JSON string for extra information
    }
}