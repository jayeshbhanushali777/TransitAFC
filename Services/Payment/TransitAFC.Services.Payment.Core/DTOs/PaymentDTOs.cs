using System.ComponentModel.DataAnnotations;
using TransitAFC.Services.Payment.Core.Models;

namespace TransitAFC.Services.Payment.Core.DTOs
{
    public class CreatePaymentRequest
    {
        [Required]
        public Guid BookingId { get; set; }

        [Required]
        [StringLength(20)]
        public string BookingNumber { get; set; } = string.Empty;

        [Required]
        [Range(0.01, double.MaxValue)]
        public decimal Amount { get; set; }

        [Required]
        public PaymentMethod Method { get; set; }

        public PaymentGateway? PreferredGateway { get; set; }

        [Required]
        [EmailAddress]
        public string CustomerEmail { get; set; } = string.Empty;

        [Required]
        [Phone]
        public string CustomerPhone { get; set; } = string.Empty;

        [StringLength(100)]
        public string? CustomerName { get; set; }

        public string? UpiId { get; set; }
        public string? Currency { get; set; } = "INR";
        public string? Notes { get; set; }
        public Dictionary<string, object>? Metadata { get; set; }

        // For recurring payments
        public bool IsRecurring { get; set; } = false;
        public int? TotalInstallments { get; set; }

        // Return URLs
        public string? SuccessUrl { get; set; }
        public string? FailureUrl { get; set; }
        public string? CancelUrl { get; set; }

        // Device information
        public string? IpAddress { get; set; }
        public string? UserAgent { get; set; }
        public string? DeviceFingerprint { get; set; }
    }

    public class PaymentResponse
    {
        public Guid Id { get; set; }
        public string PaymentId { get; set; } = string.Empty;
        public Guid UserId { get; set; }
        public Guid BookingId { get; set; }
        public string BookingNumber { get; set; } = string.Empty;
        public PaymentStatus Status { get; set; }
        public PaymentMethod Method { get; set; }
        public PaymentGateway Gateway { get; set; }
        public PaymentMode Mode { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; } = "INR";
        public decimal? TaxAmount { get; set; }
        public decimal? ServiceFee { get; set; }
        public decimal? GatewayFee { get; set; }
        public decimal? DiscountAmount { get; set; }
        public decimal TotalAmount { get; set; }
        public string? GatewayPaymentId { get; set; }
        public string? TransactionId { get; set; }
        public string? UpiId { get; set; }
        public string? CardLast4Digits { get; set; }
        public string? CardType { get; set; }
        public string? WalletType { get; set; }
        public DateTime? PaymentInitiatedAt { get; set; }
        public DateTime? PaymentCompletedAt { get; set; }
        public DateTime? ExpiresAt { get; set; }
        public string CustomerEmail { get; set; } = string.Empty;
        public string CustomerPhone { get; set; } = string.Empty;
        public string? CustomerName { get; set; }
        public string? FailureCode { get; set; }
        public string? FailureReason { get; set; }
        public bool IsRefundable { get; set; }
        public decimal RefundedAmount { get; set; }
        public int RefundCount { get; set; }
        public DateTime CreatedAt { get; set; }
        public List<PaymentTransactionResponse> Transactions { get; set; } = new();
        public List<PaymentRefundResponse> Refunds { get; set; } = new();
        public PaymentGatewayInfo? GatewayInfo { get; set; }
    }

    public class PaymentTransactionResponse
    {
        public Guid Id { get; set; }
        public string TransactionId { get; set; } = string.Empty;
        public TransactionType Type { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; } = "INR";
        public PaymentStatus Status { get; set; }
        public string? GatewayTransactionId { get; set; }
        public string? ResponseCode { get; set; }
        public string? ResponseMessage { get; set; }
        public DateTime ProcessedAt { get; set; }
        public bool IsReconciled { get; set; }
        public DateTime? SettledAt { get; set; }
    }

    public class PaymentRefundResponse
    {
        public Guid Id { get; set; }
        public string RefundId { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string Currency { get; set; } = "INR";
        public RefundStatus Status { get; set; }
        public string Reason { get; set; } = string.Empty;
        public string? GatewayRefundId { get; set; }
        public DateTime RequestedAt { get; set; }
        public DateTime? ProcessedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public string? FailureReason { get; set; }
        public int EstimatedDays { get; set; }
        public DateTime? ExpectedCompletionDate { get; set; }
        public string? RefundMethod { get; set; }
    }

    public class PaymentGatewayInfo
    {
        public string? CheckoutUrl { get; set; }
        public string? PaymentToken { get; set; }
        public string? OrderId { get; set; }
        public Dictionary<string, object>? AdditionalData { get; set; }
    }

    public class ProcessPaymentRequest
    {
        [Required]
        public string PaymentId { get; set; } = string.Empty;

        public string? GatewayPaymentId { get; set; }
        public string? GatewaySignature { get; set; }
        public string? TransactionId { get; set; }
        public Dictionary<string, object>? GatewayResponse { get; set; }
    }

    public class RefundPaymentRequest
    {
        [Required]
        [Range(0.01, double.MaxValue)]
        public decimal Amount { get; set; }

        [Required]
        [StringLength(500)]
        public string Reason { get; set; } = string.Empty;

        public string? Notes { get; set; }
        public bool IsFullRefund { get; set; } = false;
        public string? RefundMethod { get; set; }
        public string? RefundAccount { get; set; }
    }

    public class PaymentSearchRequest
    {
        public Guid? UserId { get; set; }
        public Guid? BookingId { get; set; }
        public string? PaymentId { get; set; }
        public string? BookingNumber { get; set; }
        public PaymentStatus? Status { get; set; }
        public PaymentMethod? Method { get; set; }
        public PaymentGateway? Gateway { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public decimal? MinAmount { get; set; }
        public decimal? MaxAmount { get; set; }
        public string? CustomerEmail { get; set; }
        public string? CustomerPhone { get; set; }
        public string? TransactionId { get; set; }
        public int Skip { get; set; } = 0;
        public int Take { get; set; } = 100;
    }

    public class PaymentStatsResponse
    {
        public int TotalPayments { get; set; }
        public int PendingPayments { get; set; }
        public int CompletedPayments { get; set; }
        public int FailedPayments { get; set; }
        public int RefundedPayments { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal CompletedAmount { get; set; }
        public decimal RefundedAmount { get; set; }
        public decimal PendingAmount { get; set; }
        public decimal GatewayFees { get; set; }
        public decimal NetRevenue { get; set; }
        public Dictionary<string, int> PaymentsByMethod { get; set; } = new();
        public Dictionary<string, int> PaymentsByGateway { get; set; } = new();
        public Dictionary<string, decimal> RevenueByDate { get; set; } = new();
        public Dictionary<string, int> PaymentsByStatus { get; set; } = new();
        public decimal SuccessRate { get; set; }
        public decimal AverageTransactionAmount { get; set; }
    }

    public class PaymentVerificationRequest
    {
        [Required]
        public string PaymentId { get; set; } = string.Empty;

        [Required]
        public string GatewayPaymentId { get; set; } = string.Empty;

        public string? GatewayOrderId { get; set; }
        public string? GatewaySignature { get; set; }
    }

    public class PaymentWebhookRequest
    {
        [Required]
        public PaymentGateway Gateway { get; set; }

        [Required]
        public string EventType { get; set; } = string.Empty;

        [Required]
        public Dictionary<string, object> Data { get; set; } = new();

        public string? Signature { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }

    public class UpiPaymentRequest
    {
        [Required]
        public string PaymentId { get; set; } = string.Empty;

        [Required]
        public string UpiId { get; set; } = string.Empty;

        public string? Note { get; set; }
    }

    public class CardPaymentRequest
    {
        [Required]
        public string PaymentId { get; set; } = string.Empty;

        [Required]
        public string CardToken { get; set; } = string.Empty;

        public string? SaveCard { get; set; }
        public string? CVV { get; set; }
    }

    public class WalletPaymentRequest
    {
        [Required]
        public string PaymentId { get; set; } = string.Empty;

        [Required]
        public string WalletType { get; set; } = string.Empty;

        public string? WalletPhone { get; set; }
    }

    public class NetBankingPaymentRequest
    {
        [Required]
        public string PaymentId { get; set; } = string.Empty;

        [Required]
        public string BankCode { get; set; } = string.Empty;
    }
}