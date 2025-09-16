using System.ComponentModel.DataAnnotations;
using TransitAFC.Shared.Common.Models;

namespace TransitAFC.Services.Payment.Core.Models
{
    public class Payment : BaseEntity
    {
        [Required]
        [StringLength(50)]
        public string PaymentId { get; set; } = string.Empty; // Unique payment identifier

        [Required]
        public Guid UserId { get; set; } // Reference to User service

        [Required]
        public Guid BookingId { get; set; } // Reference to Booking service

        [Required]
        [StringLength(20)]
        public string BookingNumber { get; set; } = string.Empty; // For quick reference

        [Required]
        public PaymentStatus Status { get; set; } = PaymentStatus.Pending;

        [Required]
        public PaymentMethod Method { get; set; }

        [Required]
        public PaymentGateway Gateway { get; set; }

        [Required]
        public PaymentMode Mode { get; set; } = PaymentMode.Online;

        [Required]
        public decimal Amount { get; set; }

        [Required]
        [StringLength(3)]
        public string Currency { get; set; } = "INR";

        public decimal? TaxAmount { get; set; }
        public decimal? ServiceFee { get; set; }
        public decimal? GatewayFee { get; set; }
        public decimal? DiscountAmount { get; set; }

        [Required]
        public decimal TotalAmount { get; set; }

        [StringLength(100)]
        public string? GatewayPaymentId { get; set; } // Payment ID from gateway

        [StringLength(100)]
        public string? GatewayOrderId { get; set; } // Order ID from gateway

        [StringLength(100)]
        public string? TransactionId { get; set; } // Transaction reference

        [StringLength(500)]
        public string? GatewayResponse { get; set; } // Raw gateway response

        [StringLength(100)]
        public string? PaymentToken { get; set; } // For secure payments

        // UPI specific fields
        [StringLength(50)]
        public string? UpiId { get; set; }

        [StringLength(100)]
        public string? UpiTransactionId { get; set; }

        // Card specific fields
        [StringLength(20)]
        public string? CardLast4Digits { get; set; }

        [StringLength(50)]
        public string? CardType { get; set; } // Visa, Mastercard, etc.

        [StringLength(100)]
        public string? CardIssuer { get; set; }

        // Bank transfer specific fields
        [StringLength(50)]
        public string? BankName { get; set; }

        [StringLength(20)]
        public string? BankAccountNumber { get; set; }

        [StringLength(15)]
        public string? IfscCode { get; set; }

        // Wallet specific fields
        [StringLength(50)]
        public string? WalletType { get; set; } // Paytm, PhonePe, etc.

        [StringLength(50)]
        public string? WalletTransactionId { get; set; }

        // Timing information
        public DateTime? PaymentInitiatedAt { get; set; }
        public DateTime? PaymentCompletedAt { get; set; }
        public DateTime? PaymentFailedAt { get; set; }
        public DateTime? ExpiresAt { get; set; }

        // Customer information
        [Required]
        [StringLength(100)]
        public string CustomerEmail { get; set; } = string.Empty;

        [Required]
        [StringLength(15)]
        public string CustomerPhone { get; set; } = string.Empty;

        [StringLength(100)]
        public string? CustomerName { get; set; }

        // Failure information
        [StringLength(10)]
        public string? FailureCode { get; set; }

        [StringLength(500)]
        public string? FailureReason { get; set; }

        [StringLength(1000)]
        public string? Notes { get; set; }

        // Security and compliance
        [StringLength(50)]
        public string? IpAddress { get; set; }

        [StringLength(200)]
        public string? UserAgent { get; set; }

        [StringLength(100)]
        public string? DeviceFingerprint { get; set; }

        public bool IsRefundable { get; set; } = true;
        public decimal RefundedAmount { get; set; } = 0;
        public int RefundCount { get; set; } = 0;

        // Risk assessment
        public int RiskScore { get; set; } = 0; // 0-100, higher is riskier

        [StringLength(20)]
        public string? RiskCategory { get; set; } // Low, Medium, High

        public bool IsVerificationRequired { get; set; } = false;
        public bool IsManualReviewRequired { get; set; } = false;

        // Recurring payments
        public bool IsRecurring { get; set; } = false;
        public Guid? RecurringPaymentId { get; set; }
        public int? InstallmentNumber { get; set; }
        public int? TotalInstallments { get; set; }

        // Metadata
        [StringLength(2000)]
        public string? Metadata { get; set; } // JSON string for additional data

        // Navigation properties
        public ICollection<PaymentTransaction> Transactions { get; set; } = new List<PaymentTransaction>();
        public ICollection<PaymentRefund> Refunds { get; set; } = new List<PaymentRefund>();
        public ICollection<PaymentHistory> PaymentHistory { get; set; } = new List<PaymentHistory>();
    }
}