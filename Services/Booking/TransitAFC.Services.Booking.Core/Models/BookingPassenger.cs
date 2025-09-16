using System.ComponentModel.DataAnnotations;
using TransitAFC.Shared.Common.Models;

namespace TransitAFC.Services.Booking.Core.Models
{
    public class BookingPassenger : BaseEntity
    {
        [Required]
        public Guid BookingId { get; set; }
        public Booking Booking { get; set; } = null!;

        [Required]
        [StringLength(100)]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string LastName { get; set; } = string.Empty;

        [Required]
        public PassengerType PassengerType { get; set; } = PassengerType.Adult;

        [Range(0, 120)]
        public int? Age { get; set; }

        [StringLength(10)]
        public string? Gender { get; set; } // M, F, O

        [StringLength(50)]
        public string? SeatNumber { get; set; }

        public SeatType? SeatType { get; set; }

        public decimal Fare { get; set; }

        public decimal? DiscountAmount { get; set; }

        public decimal FinalFare { get; set; }

        [StringLength(50)]
        public string? IdentityType { get; set; } // Aadhar, PAN, Passport, etc.

        [StringLength(50)]
        public string? IdentityNumber { get; set; }

        [StringLength(15)]
        public string? ContactPhone { get; set; }

        [StringLength(100)]
        public string? ContactEmail { get; set; }

        [StringLength(500)]
        public string? SpecialRequests { get; set; }

        public bool HasWheelchairAccess { get; set; } = false;

        public bool IsPrimaryPassenger { get; set; } = false; // Main contact person

        [StringLength(20)]
        public string? TicketNumber { get; set; } // Individual ticket number

        public DateTime? CheckInTime { get; set; }
        public DateTime? CheckOutTime { get; set; }

        public string FullName => $"{FirstName} {LastName}";
    }
}