using System.ComponentModel.DataAnnotations;
using TransitAFC.Shared.Common.Models;

namespace TransitAFC.Services.Ticket.Core.Models
{
    public class Ticket : BaseEntity
    {
        [Required]
        [StringLength(50)]
        public string TicketNumber { get; set; } = string.Empty; // Unique ticket identifier

        [Required]
        public Guid UserId { get; set; } // Reference to User service

        [Required]
        public Guid BookingId { get; set; } // Reference to Booking service

        [Required]
        public Guid PaymentId { get; set; } // Reference to Payment service

        [Required]
        [StringLength(20)]
        public string BookingNumber { get; set; } = string.Empty; // For quick reference

        [Required]
        public TicketStatus Status { get; set; } = TicketStatus.Draft;

        [Required]
        public TicketType Type { get; set; }

        [Required]
        public TransportMode TransportMode { get; set; }

        [Required]
        public FareType FareType { get; set; } = FareType.Standard;

        // Route Information
        [Required]
        public Guid SourceStationId { get; set; }

        [Required]
        [StringLength(100)]
        public string SourceStationName { get; set; } = string.Empty;

        [Required]
        [StringLength(10)]
        public string SourceStationCode { get; set; } = string.Empty;

        [Required]
        public Guid DestinationStationId { get; set; }

        [Required]
        [StringLength(100)]
        public string DestinationStationName { get; set; } = string.Empty;

        [Required]
        [StringLength(10)]
        public string DestinationStationCode { get; set; } = string.Empty;

        [Required]
        public Guid RouteId { get; set; }

        [StringLength(100)]
        public string RouteName { get; set; } = string.Empty;

        [StringLength(20)]
        public string RouteCode { get; set; } = string.Empty;

        // Pricing Information
        [Required]
        public decimal BasePrice { get; set; }

        public decimal? DiscountAmount { get; set; }

        public decimal? TaxAmount { get; set; }

        [Required]
        public decimal FinalPrice { get; set; }

        [Required]
        [StringLength(3)]
        public string Currency { get; set; } = "INR";

        // Timing Information
        [Required]
        public DateTime ValidFrom { get; set; }

        [Required]
        public DateTime ValidUntil { get; set; }

        public DateTime? FirstUsedAt { get; set; }
        public DateTime? LastUsedAt { get; set; }

        // Journey Information
        public int MaxUsageCount { get; set; } = 1; // Number of times ticket can be used
        public int UsageCount { get; set; } = 0; // Current usage count

        public decimal EstimatedDuration { get; set; } = 0; // In minutes
        public decimal EstimatedDistance { get; set; } = 0; // In kilometers

        // Passenger Information
        [Required]
        [StringLength(100)]
        public string PassengerName { get; set; } = string.Empty;

        [StringLength(20)]
        public string PassengerAge { get; set; } = string.Empty;

        [StringLength(20)]
        public string PassengerType { get; set; } = "Adult"; // Adult, Child, Senior, Student

        [StringLength(15)]
        public string PassengerPhone { get; set; } = string.Empty;

        [StringLength(100)]
        public string PassengerEmail { get; set; } = string.Empty;

        // QR Code Information
        [Required]
        [StringLength(500)]
        public string QRCodeData { get; set; } = string.Empty; // Encrypted QR code data

        [StringLength(100)]
        public string QRCodeHash { get; set; } = string.Empty; // Hash for validation

        public byte[]? QRCodeImage { get; set; } // QR code image bytes

        // Validation Information
        public Guid? ValidatedByStationId { get; set; }
        [StringLength(100)]
        public string? ValidatedByStationName { get; set; }

        public Guid? ValidatedByDeviceId { get; set; }
        [StringLength(50)]
        public string? ValidatedByDeviceName { get; set; }

        public Guid? ValidatedByOperatorId { get; set; }
        [StringLength(100)]
        public string? ValidatedByOperatorName { get; set; }

        // Zone and Class Information
        [StringLength(20)]
        public string? ZoneFrom { get; set; }

        [StringLength(20)]
        public string? ZoneTo { get; set; }

        [StringLength(20)]
        public string? ServiceClass { get; set; } = "Standard"; // Standard, Premium, Economy

        [StringLength(10)]
        public string? SeatNumber { get; set; }

        [StringLength(5)]
        public string? CoachNumber { get; set; }

        // Additional Information
        [StringLength(500)]
        public string? SpecialInstructions { get; set; }

        [StringLength(200)]
        public string? CancellationReason { get; set; }

        [StringLength(1000)]
        public string? Notes { get; set; }

        // Transfer Information
        public bool AllowsTransfer { get; set; } = false;
        public int MaxTransfers { get; set; } = 0;
        public int TransferCount { get; set; } = 0;
        public decimal? TransferTimeLimit { get; set; } // In minutes

        // Metadata
        [StringLength(2000)]
        public string? Metadata { get; set; } // JSON string for additional data

        // Compliance and Tracking
        [StringLength(50)]
        public string? RegulatoryCode { get; set; }

        [StringLength(100)]
        public string? OperatorCode { get; set; }

        public bool IsRefundable { get; set; } = true;
        public bool IsTransferable { get; set; } = false;
        public bool RequiresValidation { get; set; } = true;

        // Navigation properties
        public ICollection<TicketValidation> Validations { get; set; } = new List<TicketValidation>();
        public ICollection<TicketQRCode> QRCodes { get; set; } = new List<TicketQRCode>();
        public ICollection<TicketHistory> TicketHistory { get; set; } = new List<TicketHistory>();
        public ICollection<TicketTransfer> Transfers { get; set; } = new List<TicketTransfer>();
    }
}