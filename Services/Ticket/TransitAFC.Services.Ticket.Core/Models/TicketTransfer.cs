using System.ComponentModel.DataAnnotations;
using TransitAFC.Shared.Common.Models;

namespace TransitAFC.Services.Ticket.Core.Models
{
    public class TicketTransfer : BaseEntity
    {
        [Required]
        public Guid TicketId { get; set; }
        public Ticket Ticket { get; set; } = null!;

        [Required]
        [StringLength(50)]
        public string TransferId { get; set; } = string.Empty; // Unique transfer identifier

        [Required]
        public int TransferSequence { get; set; } // 1st transfer, 2nd transfer, etc.

        // From Information
        [Required]
        public Guid FromStationId { get; set; }

        [Required]
        [StringLength(100)]
        public string FromStationName { get; set; } = string.Empty;

        [Required]
        [StringLength(10)]
        public string FromStationCode { get; set; } = string.Empty;

        [Required]
        public TransportMode FromTransportMode { get; set; }

        [StringLength(50)]
        public string? FromRouteNumber { get; set; }

        [StringLength(20)]
        public string? FromVehicleNumber { get; set; }

        public DateTime ExitTime { get; set; }

        // To Information
        [Required]
        public Guid ToStationId { get; set; }

        [Required]
        [StringLength(100)]
        public string ToStationName { get; set; } = string.Empty;

        [Required]
        [StringLength(10)]
        public string ToStationCode { get; set; } = string.Empty;

        [Required]
        public TransportMode ToTransportMode { get; set; }

        [StringLength(50)]
        public string? ToRouteNumber { get; set; }

        [StringLength(20)]
        public string? ToVehicleNumber { get; set; }

        public DateTime EntryTime { get; set; }

        // Transfer Details
        public decimal TransferTime { get; set; } = 0; // In minutes
        public decimal WalkingDistance { get; set; } = 0; // In meters

        public bool IsValidTransfer { get; set; } = true;
        public bool IsWithinTimeLimit { get; set; } = true;
        public bool IsWithinDistance { get; set; } = true;

        // Additional Charges
        public decimal TransferFee { get; set; } = 0;
        public decimal AdditionalFare { get; set; } = 0;

        [StringLength(200)]
        public string? TransferReason { get; set; }

        [StringLength(500)]
        public string? TransferNotes { get; set; }

        // Validation Information
        public Guid? ExitValidationId { get; set; }
        public Guid? EntryValidationId { get; set; }

        [StringLength(50)]
        public string? TransferCode { get; set; } // Code for free/discounted transfer

        // Operator Information
        public Guid? AuthorizedBy { get; set; }

        [StringLength(100)]
        public string? AuthorizedByName { get; set; }

        [StringLength(20)]
        public string? AuthorizedByBadge { get; set; }
    }
}