using System.ComponentModel.DataAnnotations;
using TransitAFC.Services.Ticket.Core.Models;

namespace TransitAFC.Services.Ticket.Core.DTOs
{
    public class CreateTicketRequest
    {
        [Required]
        public Guid BookingId { get; set; }

        [Required]
        public Guid PaymentId { get; set; }

        [Required]
        [StringLength(20)]
        public string BookingNumber { get; set; } = string.Empty;

        [Required]
        public TicketType Type { get; set; }

        [Required]
        public TransportMode TransportMode { get; set; }

        public FareType FareType { get; set; } = FareType.Standard;

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
        public string? RouteName { get; set; }

        [StringLength(20)]
        public string? RouteCode { get; set; }

        [Required]
        [Range(0.01, double.MaxValue)]
        public decimal BasePrice { get; set; }

        public decimal? DiscountAmount { get; set; }
        public decimal? TaxAmount { get; set; }

        [Required]
        [Range(0.01, double.MaxValue)]
        public decimal FinalPrice { get; set; }

        public string Currency { get; set; } = "INR";

        [Required]
        public DateTime ValidFrom { get; set; }

        [Required]
        public DateTime ValidUntil { get; set; }

        public int MaxUsageCount { get; set; } = 1;

        [Required]
        [StringLength(100)]
        public string PassengerName { get; set; } = string.Empty;

        [StringLength(20)]
        public string? PassengerAge { get; set; }

        [StringLength(20)]
        public string PassengerType { get; set; } = "Adult";

        [StringLength(15)]
        public string? PassengerPhone { get; set; }

        [StringLength(100)]
        public string? PassengerEmail { get; set; }

        public bool AllowsTransfer { get; set; } = false;
        public int MaxTransfers { get; set; } = 0;
        public decimal? TransferTimeLimit { get; set; }

        [StringLength(20)]
        public string? ServiceClass { get; set; } = "Standard";

        [StringLength(10)]
        public string? SeatNumber { get; set; }

        [StringLength(5)]
        public string? CoachNumber { get; set; }

        [StringLength(500)]
        public string? SpecialInstructions { get; set; }

        public Dictionary<string, object>? Metadata { get; set; }
    }

    public class TicketResponse
    {
        public Guid Id { get; set; }
        public string TicketNumber { get; set; } = string.Empty;
        public Guid UserId { get; set; }
        public Guid BookingId { get; set; }
        public Guid PaymentId { get; set; }
        public string BookingNumber { get; set; } = string.Empty;
        public TicketStatus Status { get; set; }
        public TicketType Type { get; set; }
        public TransportMode TransportMode { get; set; }
        public FareType FareType { get; set; }

        // Route Information
        public Guid SourceStationId { get; set; }
        public string SourceStationName { get; set; } = string.Empty;
        public string SourceStationCode { get; set; } = string.Empty;
        public Guid DestinationStationId { get; set; }
        public string DestinationStationName { get; set; } = string.Empty;
        public string DestinationStationCode { get; set; } = string.Empty;
        public Guid RouteId { get; set; }
        public string RouteName { get; set; } = string.Empty;
        public string RouteCode { get; set; } = string.Empty;

        // Pricing
        public decimal BasePrice { get; set; }
        public decimal? DiscountAmount { get; set; }
        public decimal? TaxAmount { get; set; }
        public decimal FinalPrice { get; set; }
        public string Currency { get; set; } = "INR";

        // Timing
        public DateTime ValidFrom { get; set; }
        public DateTime ValidUntil { get; set; }
        public DateTime? FirstUsedAt { get; set; }
        public DateTime? LastUsedAt { get; set; }

        // Usage
        public int MaxUsageCount { get; set; }
        public int UsageCount { get; set; }
        public decimal EstimatedDuration { get; set; }
        public decimal EstimatedDistance { get; set; }

        // Passenger
        public string PassengerName { get; set; } = string.Empty;
        public string? PassengerAge { get; set; }
        public string PassengerType { get; set; } = "Adult";
        public string? PassengerPhone { get; set; }
        public string? PassengerEmail { get; set; }

        // QR Code
        public string QRCodeData { get; set; } = string.Empty;
        public byte[]? QRCodeImage { get; set; }

        // Additional Info
        public bool AllowsTransfer { get; set; }
        public int MaxTransfers { get; set; }
        public int TransferCount { get; set; }
        public decimal? TransferTimeLimit { get; set; }
        public string? ServiceClass { get; set; }
        public string? SeatNumber { get; set; }
        public string? CoachNumber { get; set; }
        public string? SpecialInstructions { get; set; }
        public bool IsRefundable { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        // Related Data
        public List<TicketValidationResponse> Validations { get; set; } = new();
        public List<TicketQRCodeResponse> QRCodes { get; set; } = new();
        public List<TicketTransferResponse> Transfers { get; set; } = new();
    }

    public class TicketValidationResponse
    {
        public Guid Id { get; set; }
        public string ValidationId { get; set; } = string.Empty;
        public TicketValidationType ValidationType { get; set; }
        public Models.ValidationResult ValidationResult { get; set; }
        public DateTime ValidationTime { get; set; }
        public Guid StationId { get; set; }
        public string StationName { get; set; } = string.Empty;
        public string StationCode { get; set; } = string.Empty;
        public Guid DeviceId { get; set; }
        public string DeviceName { get; set; } = string.Empty;
        public string? DeviceType { get; set; }
        public Guid? OperatorId { get; set; }
        public string? OperatorName { get; set; }
        public string? ValidationMethod { get; set; }
        public string? ValidationError { get; set; }
        public bool IsSuccessful { get; set; }
        public decimal? FareDeducted { get; set; }
        public string? VehicleNumber { get; set; }
        public string? RouteNumber { get; set; }
    }

    public class TicketQRCodeResponse
    {
        public Guid Id { get; set; }
        public string QRCodeId { get; set; } = string.Empty;
        public byte[] QRCodeImage { get; set; } = Array.Empty<byte>();
        public QRCodeStatus Status { get; set; }
        public DateTime GeneratedAt { get; set; }
        public DateTime ExpiresAt { get; set; }
        public DateTime? UsedAt { get; set; }
        public int ScanCount { get; set; }
        public DateTime? LastScannedAt { get; set; }
        public string? DisplayText { get; set; }
        public string? Instructions { get; set; }
    }

    public class TicketTransferResponse
    {
        public Guid Id { get; set; }
        public string TransferId { get; set; } = string.Empty;
        public int TransferSequence { get; set; }
        public Guid FromStationId { get; set; }
        public string FromStationName { get; set; } = string.Empty;
        public string FromStationCode { get; set; } = string.Empty;
        public TransportMode FromTransportMode { get; set; }
        public string? FromRouteNumber { get; set; }
        public DateTime ExitTime { get; set; }
        public Guid ToStationId { get; set; }
        public string ToStationName { get; set; } = string.Empty;
        public string ToStationCode { get; set; } = string.Empty;
        public TransportMode ToTransportMode { get; set; }
        public string? ToRouteNumber { get; set; }
        public DateTime EntryTime { get; set; }
        public decimal TransferTime { get; set; }
        public bool IsValidTransfer { get; set; }
        public decimal TransferFee { get; set; }
        public decimal AdditionalFare { get; set; }
    }

    public class ValidateTicketRequest
    {
        [Required]
        [StringLength(500)]
        public string QRCodeData { get; set; } = string.Empty;

        [Required]
        public Guid StationId { get; set; }

        [Required]
        [StringLength(100)]
        public string StationName { get; set; } = string.Empty;

        [Required]
        [StringLength(10)]
        public string StationCode { get; set; } = string.Empty;

        [Required]
        public Guid DeviceId { get; set; }

        [Required]
        [StringLength(50)]
        public string DeviceName { get; set; } = string.Empty;

        [StringLength(20)]
        public string? DeviceType { get; set; } = "Turnstile";

        public Guid? OperatorId { get; set; }

        [StringLength(100)]
        public string? OperatorName { get; set; }

        [Required]
        public TicketValidationType ValidationType { get; set; } = TicketValidationType.Entry;

        [StringLength(100)]
        public string ValidationMethod { get; set; } = "QR_SCAN";

        public decimal? Latitude { get; set; }
        public decimal? Longitude { get; set; }

        [StringLength(20)]
        public string? Direction { get; set; }

        [StringLength(50)]
        public string? Platform { get; set; }

        [StringLength(20)]
        public string? Gate { get; set; }

        public Guid? TripId { get; set; }

        [StringLength(20)]
        public string? VehicleNumber { get; set; }

        [StringLength(50)]
        public string? RouteNumber { get; set; }
    }

    public class TicketValidationResult
    {
        public bool IsValid { get; set; }
        public Models.ValidationResult Result { get; set; }
        public string Message { get; set; } = string.Empty;
        public Guid? TicketId { get; set; }
        public string? TicketNumber { get; set; }
        public TicketStatus? TicketStatus { get; set; }
        public TicketType? TicketType { get; set; }
        public string? PassengerName { get; set; }
        public DateTime? ValidUntil { get; set; }
        public int? RemainingUsage { get; set; }
        public decimal? FareDeducted { get; set; }
        public decimal? BalanceAfter { get; set; }
        public bool AllowsTransfer { get; set; }
        public int? TransfersRemaining { get; set; }
        public string? AdditionalInfo { get; set; }
        public DateTime ValidationTime { get; set; }
        public Guid ValidationId { get; set; }
    }

    public class TicketSearchRequest
    {
        public Guid? UserId { get; set; }
        public string? TicketNumber { get; set; }
        public Guid? BookingId { get; set; }
        public string? BookingNumber { get; set; }
        public TicketStatus? Status { get; set; }
        public TicketType? Type { get; set; }
        public TransportMode? TransportMode { get; set; }
        public Guid? SourceStationId { get; set; }
        public Guid? DestinationStationId { get; set; }
        public DateTime? ValidFromDate { get; set; }
        public DateTime? ValidToDate { get; set; }
        public DateTime? CreatedFromDate { get; set; }
        public DateTime? CreatedToDate { get; set; }
        public string? PassengerName { get; set; }
        public string? PassengerPhone { get; set; }
        public string? PassengerEmail { get; set; }
        public int Skip { get; set; } = 0;
        public int Take { get; set; } = 100;
    }

    public class TicketStatsResponse
    {
        public int TotalTickets { get; set; }
        public int ActiveTickets { get; set; }
        public int UsedTickets { get; set; }
        public int ExpiredTickets { get; set; }
        public int CancelledTickets { get; set; }
        public decimal TotalRevenue { get; set; }
        public decimal AverageTicketPrice { get; set; }
        public Dictionary<string, int> TicketsByType { get; set; } = new();
        public Dictionary<string, int> TicketsByStatus { get; set; } = new();
        public Dictionary<string, int> TicketsByTransportMode { get; set; } = new();
        public Dictionary<string, decimal> RevenueByDate { get; set; } = new();
        public Dictionary<string, int> UsageByStation { get; set; } = new();
        public decimal UsageRate { get; set; }
        public int TotalValidations { get; set; }
        public int SuccessfulValidations { get; set; }
        public int FailedValidations { get; set; }
        public decimal ValidationSuccessRate { get; set; }
    }

    public class CancelTicketRequest
    {
        [Required]
        [StringLength(200)]
        public string Reason { get; set; } = string.Empty;

        [StringLength(1000)]
        public string? Notes { get; set; }

        public bool RequestRefund { get; set; } = false;
    }

    public class RegenerateQRRequest
    {
        [StringLength(200)]
        public string? Reason { get; set; }

        public int? Size { get; set; } = 300;

        [StringLength(20)]
        public string? Format { get; set; } = "PNG";
    }

    public class BulkTicketOperation
    {
        [Required]
        public List<Guid> TicketIds { get; set; } = new();

        [Required]
        [StringLength(50)]
        public string Operation { get; set; } = string.Empty; // Cancel, Activate, Suspend

        [StringLength(500)]
        public string? Reason { get; set; }

        [StringLength(1000)]
        public string? Notes { get; set; }
    }

    public class TicketTransferRequest
    {
        [Required]
        public Guid FromStationId { get; set; }

        [Required]
        public Guid ToStationId { get; set; }

        [Required]
        public TransportMode FromTransportMode { get; set; }

        [Required]
        public TransportMode ToTransportMode { get; set; }

        [StringLength(50)]
        public string? FromRouteNumber { get; set; }

        [StringLength(50)]
        public string? ToRouteNumber { get; set; }

        [StringLength(20)]
        public string? FromVehicleNumber { get; set; }

        [StringLength(20)]
        public string? ToVehicleNumber { get; set; }

        public Guid? ExitValidationId { get; set; }
        public Guid? EntryValidationId { get; set; }

        [StringLength(50)]
        public string? TransferCode { get; set; }

        [StringLength(200)]
        public string? TransferReason { get; set; }
    }
}