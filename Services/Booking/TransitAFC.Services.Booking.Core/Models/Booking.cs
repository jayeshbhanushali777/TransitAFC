using System.ComponentModel.DataAnnotations;
using TransitAFC.Shared.Common.Models;

namespace TransitAFC.Services.Booking.Core.Models
{
    public class Booking : BaseEntity
    {
        [Required]
        [StringLength(20)]
        public string BookingNumber { get; set; } = string.Empty; // Unique booking reference

        [Required]
        public Guid UserId { get; set; } // Reference to User service

        [Required]
        public Guid RouteId { get; set; } // Reference to Route service

        [Required]
        [StringLength(20)]
        public string RouteCode { get; set; } = string.Empty; // For quick reference

        [Required]
        [StringLength(100)]
        public string RouteName { get; set; } = string.Empty;

        [Required]
        public Guid SourceStationId { get; set; }

        [Required]
        [StringLength(100)]
        public string SourceStationName { get; set; } = string.Empty;

        [Required]
        public Guid DestinationStationId { get; set; }

        [Required]
        [StringLength(100)]
        public string DestinationStationName { get; set; } = string.Empty;

        [Required]
        public DateTime DepartureTime { get; set; }

        public DateTime? ArrivalTime { get; set; }

        [Required]
        public BookingStatus Status { get; set; } = BookingStatus.Draft;

        [Required]
        public int PassengerCount { get; set; }

        [Required]
        public decimal TotalFare { get; set; }

        public decimal? DiscountAmount { get; set; }

        public decimal? TaxAmount { get; set; }

        [Required]
        public decimal FinalAmount { get; set; }

        [StringLength(50)]
        public string? DiscountCode { get; set; }

        [StringLength(50)]
        public string Currency { get; set; } = "INR";

        [Required]
        [StringLength(100)]
        public string ContactEmail { get; set; } = string.Empty;

        [Required]
        [StringLength(15)]
        public string ContactPhone { get; set; } = string.Empty;

        [StringLength(500)]
        public string? SpecialRequests { get; set; }

        [StringLength(1000)]
        public string? Notes { get; set; }

        public DateTime? ConfirmedAt { get; set; }
        public DateTime? CancelledAt { get; set; }
        public DateTime? CompletedAt { get; set; }

        [StringLength(500)]
        public string? CancellationReason { get; set; }

        public Guid? PaymentId { get; set; } // Reference to Payment service
        public Guid? TicketId { get; set; } // Reference to Ticket service

        [StringLength(50)]
        public string? VehicleNumber { get; set; }

        [StringLength(100)]
        public string? SeatNumbers { get; set; } // JSON array of seat numbers

        public bool IsRoundTrip { get; set; } = false;
        public Guid? ReturnBookingId { get; set; } // For return journey

        public DateTime? BookingExpiresAt { get; set; } // Expiry for pending bookings

        // Metadata
        [StringLength(50)]
        public string? BookingSource { get; set; } = "WebApp"; // WebApp, MobileApp, API

        [StringLength(100)]
        public string? DeviceInfo { get; set; }

        [StringLength(50)]
        public string? IpAddress { get; set; }

        // Navigation properties
        public ICollection<BookingPassenger> Passengers { get; set; } = new List<BookingPassenger>();
        public ICollection<BookingHistory> BookingHistory { get; set; } = new List<BookingHistory>();
    }
}