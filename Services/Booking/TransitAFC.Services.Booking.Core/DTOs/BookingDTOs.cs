using System.ComponentModel.DataAnnotations;
using TransitAFC.Services.Booking.Core.Models;

namespace TransitAFC.Services.Booking.Core.DTOs
{
    public class CreateBookingRequest
    {
        [Required]
        public Guid RouteId { get; set; }

        [Required]
        public Guid SourceStationId { get; set; }

        [Required]
        public Guid DestinationStationId { get; set; }

        [Required]
        public DateTime DepartureTime { get; set; }

        [Required]
        [EmailAddress]
        public string ContactEmail { get; set; } = string.Empty;

        [Required]
        [Phone]
        public string ContactPhone { get; set; } = string.Empty;

        [Required]
        [MinLength(1)]
        public List<CreatePassengerRequest> Passengers { get; set; } = new();

        public string? SpecialRequests { get; set; }
        public string? DiscountCode { get; set; }
        public bool IsRoundTrip { get; set; } = false;
        public DateTime? ReturnDepartureTime { get; set; }
        public List<string>? PreferredSeats { get; set; }
        public string? BookingSource { get; set; } = "WebApp";
    }

    public class CreatePassengerRequest
    {
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
        public string? Gender { get; set; }

        public SeatType? PreferredSeatType { get; set; }

        public string? IdentityType { get; set; }
        public string? IdentityNumber { get; set; }
        public string? ContactPhone { get; set; }
        public string? ContactEmail { get; set; }
        public string? SpecialRequests { get; set; }
        public bool HasWheelchairAccess { get; set; } = false;
        public bool IsPrimaryPassenger { get; set; } = false;
    }

    public class BookingResponse
    {
        public Guid Id { get; set; }
        public string BookingNumber { get; set; } = string.Empty;
        public Guid UserId { get; set; }
        public RouteInfo Route { get; set; } = new();
        public StationInfo SourceStation { get; set; } = new();
        public StationInfo DestinationStation { get; set; } = new();
        public DateTime DepartureTime { get; set; }
        public DateTime? ArrivalTime { get; set; }
        public BookingStatus Status { get; set; }
        public int PassengerCount { get; set; }
        public decimal TotalFare { get; set; }
        public decimal? DiscountAmount { get; set; }
        public decimal? TaxAmount { get; set; }
        public decimal FinalAmount { get; set; }
        public string? DiscountCode { get; set; }
        public string Currency { get; set; } = "INR";
        public string ContactEmail { get; set; } = string.Empty;
        public string ContactPhone { get; set; } = string.Empty;
        public string? SpecialRequests { get; set; }
        public DateTime? ConfirmedAt { get; set; }
        public DateTime? CancelledAt { get; set; }
        public DateTime? BookingExpiresAt { get; set; }
        public List<PassengerResponse> Passengers { get; set; } = new();
        public PaymentInfo? Payment { get; set; }
        public TicketInfo? Ticket { get; set; }
        public string? VehicleNumber { get; set; }
        public List<string> SeatNumbers { get; set; } = new();
        public bool IsRoundTrip { get; set; }
        public BookingResponse? ReturnBooking { get; set; }
        public DateTime CreatedAt { get; set; }
        public string BookingSource { get; set; } = "WebApp";
    }

    public class PassengerResponse
    {
        public Guid Id { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public PassengerType PassengerType { get; set; }
        public int? Age { get; set; }
        public string? Gender { get; set; }
        public string? SeatNumber { get; set; }
        public SeatType? SeatType { get; set; }
        public decimal Fare { get; set; }
        public decimal? DiscountAmount { get; set; }
        public decimal FinalFare { get; set; }
        public string? IdentityType { get; set; }
        public string? IdentityNumber { get; set; }
        public string? ContactPhone { get; set; }
        public string? ContactEmail { get; set; }
        public bool HasWheelchairAccess { get; set; }
        public bool IsPrimaryPassenger { get; set; }
        public string? TicketNumber { get; set; }
        public DateTime? CheckInTime { get; set; }
        public DateTime? CheckOutTime { get; set; }
    }

    public class RouteInfo
    {
        public Guid Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string TransportMode { get; set; } = string.Empty;
        public decimal Distance { get; set; }
        public TimeSpan EstimatedDuration { get; set; }
        public decimal BaseFare { get; set; }
    }

    public class StationInfo
    {
        public Guid Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public double Latitude { get; set; }
        public double Longitude { get; set; }
    }

    public class PaymentInfo
    {
        public Guid PaymentId { get; set; }
        public string PaymentMethod { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime? PaidAt { get; set; }
        public string? TransactionId { get; set; }
    }

    public class TicketInfo
    {
        public Guid TicketId { get; set; }
        public string TicketNumber { get; set; } = string.Empty;
        public string QrCode { get; set; } = string.Empty;
        public DateTime? GeneratedAt { get; set; }
        public bool IsUsed { get; set; }
    }

    public class BookingSearchRequest
    {
        public Guid? UserId { get; set; }
        public string? BookingNumber { get; set; }
        public BookingStatus? Status { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public Guid? RouteId { get; set; }
        public string? ContactEmail { get; set; }
        public string? ContactPhone { get; set; }
        public int Skip { get; set; } = 0;
        public int Take { get; set; } = 100;
    }

    public class UpdateBookingRequest
    {
        public string? ContactEmail { get; set; }
        public string? ContactPhone { get; set; }
        public string? SpecialRequests { get; set; }
        public List<UpdatePassengerRequest>? Passengers { get; set; }
    }

    public class UpdatePassengerRequest
    {
        public Guid PassengerId { get; set; }
        public string? ContactPhone { get; set; }
        public string? ContactEmail { get; set; }
        public string? SpecialRequests { get; set; }
        public string? SeatNumber { get; set; }
    }

    public class CancelBookingRequest
    {
        [Required]
        public string Reason { get; set; } = string.Empty;
        public string? Notes { get; set; }
    }

    public class ConfirmBookingRequest
    {
        [Required]
        public Guid PaymentId { get; set; }
        public List<SeatAssignment>? SeatAssignments { get; set; }
        public string? VehicleNumber { get; set; }
    }

    public class SeatAssignment
    {
        [Required]
        public Guid PassengerId { get; set; }
        [Required]
        public string SeatNumber { get; set; } = string.Empty;
        public SeatType? SeatType { get; set; }
    }

    public class BookingStatsResponse
    {
        public int TotalBookings { get; set; }
        public int PendingBookings { get; set; }
        public int ConfirmedBookings { get; set; }
        public int CancelledBookings { get; set; }
        public int CompletedBookings { get; set; }
        public decimal TotalRevenue { get; set; }
        public decimal PendingRevenue { get; set; }
        public int TotalPassengers { get; set; }
        public Dictionary<string, int> BookingsByRoute { get; set; } = new();
        public Dictionary<string, int> BookingsByStatus { get; set; } = new();
        public Dictionary<string, decimal> RevenueByDate { get; set; } = new();
    }

    public class FareCalculationRequest
    {
        [Required]
        public Guid RouteId { get; set; }

        [Required]
        public Guid SourceStationId { get; set; }

        [Required]
        public Guid DestinationStationId { get; set; }

        [Required]
        public List<PassengerType> PassengerTypes { get; set; } = new();

        public string? DiscountCode { get; set; }
        public DateTime? TravelDate { get; set; }
    }

    public class FareCalculationResponse
    {
        public decimal BaseFare { get; set; }
        public List<PassengerFare> PassengerFares { get; set; } = new();
        public decimal TotalFare { get; set; }
        public decimal? DiscountAmount { get; set; }
        public decimal? TaxAmount { get; set; }
        public decimal FinalAmount { get; set; }
        public string Currency { get; set; } = "INR";
        public List<string> AppliedDiscounts { get; set; } = new();
        public FareBreakdown Breakdown { get; set; } = new();
    }

    public class PassengerFare
    {
        public PassengerType PassengerType { get; set; }
        public decimal BaseFare { get; set; }
        public decimal? DiscountAmount { get; set; }
        public decimal FinalFare { get; set; }
        public string DiscountReason { get; set; } = string.Empty;
    }

    public class FareBreakdown
    {
        public decimal Distance { get; set; }
        public decimal RatePerKm { get; set; }
        public decimal BaseFare { get; set; }
        public decimal ServiceTax { get; set; }
        public decimal PlatformFee { get; set; }
        public decimal ConvenienceFee { get; set; }
        public decimal TotalTax { get; set; }
        public decimal GrandTotal { get; set; }
    }
}