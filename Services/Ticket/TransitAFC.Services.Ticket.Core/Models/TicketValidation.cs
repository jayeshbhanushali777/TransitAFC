using System.ComponentModel.DataAnnotations;
using TransitAFC.Shared.Common.Models;

namespace TransitAFC.Services.Ticket.Core.Models
{
    public class TicketValidation : BaseEntity
    {
        [Required]
        public Guid TicketId { get; set; }
        public Ticket Ticket { get; set; } = null!;

        [Required]
        [StringLength(50)]
        public string ValidationId { get; set; } = string.Empty; // Unique validation identifier

        [Required]
        public TicketValidationType ValidationType { get; set; }

        [Required]
        public ValidationResult ValidationResult { get; set; }

        [Required]
        public DateTime ValidationTime { get; set; } = DateTime.UtcNow;

        // Station Information
        [Required]
        public Guid StationId { get; set; }

        [Required]
        [StringLength(100)]
        public string StationName { get; set; } = string.Empty;

        [Required]
        [StringLength(10)]
        public string StationCode { get; set; } = string.Empty;

        [StringLength(20)]
        public string? ZoneCode { get; set; }

        // Device Information
        [Required]
        public Guid DeviceId { get; set; }

        [Required]
        [StringLength(50)]
        public string DeviceName { get; set; } = string.Empty;

        [StringLength(20)]
        public string? DeviceType { get; set; } // Turnstile, Handheld, Mobile, Kiosk

        [StringLength(50)]
        public string? DeviceSerialNumber { get; set; }

        // Operator Information
        public Guid? OperatorId { get; set; }

        [StringLength(100)]
        public string? OperatorName { get; set; }

        [StringLength(20)]
        public string? OperatorBadgeNumber { get; set; }

        // Validation Details
        [StringLength(100)]
        public string? ValidationMethod { get; set; } // QR_SCAN, NFC, MANUAL, BIOMETRIC

        [StringLength(500)]
        public string? ValidationData { get; set; } // Additional validation data

        [StringLength(500)]
        public string? ValidationError { get; set; } // Error message if validation failed

        public bool IsSuccessful => ValidationResult == ValidationResult.Valid;

        // Location Information
        public decimal? Latitude { get; set; }
        public decimal? Longitude { get; set; }

        [StringLength(200)]
        public string? LocationDescription { get; set; }

        // Journey Information
        [StringLength(20)]
        public string? Direction { get; set; } // Inbound, Outbound

        [StringLength(50)]
        public string? Platform { get; set; }

        [StringLength(20)]
        public string? Gate { get; set; }

        // Fare Information
        public decimal? FareDeducted { get; set; }
        public decimal? BalanceAfter { get; set; }

        // Trip Information
        public Guid? TripId { get; set; } // If part of a specific trip/service

        [StringLength(20)]
        public string? VehicleNumber { get; set; }

        [StringLength(50)]
        public string? RouteNumber { get; set; }

        // System Information
        [StringLength(50)]
        public string? SystemVersion { get; set; }

        [StringLength(100)]
        public string? ProcessingTime { get; set; } // Time taken to process validation

        [StringLength(50)]
        public string? TransactionId { get; set; } // For financial reconciliation

        // Compliance
        public bool RequiresManualReview { get; set; } = false;

        [StringLength(500)]
        public string? ReviewNotes { get; set; }

        public DateTime? ReviewedAt { get; set; }
        public Guid? ReviewedBy { get; set; }
    }
}