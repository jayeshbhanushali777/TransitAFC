using System.ComponentModel.DataAnnotations;
using TransitAFC.Shared.Common.Models;

namespace TransitAFC.Services.Payment.Core.Models
{
    public class PaymentRefund : BaseEntity
    {
        [Required]
        public Guid PaymentId { get; set; }
        public Payment Payment { get; set; } = null!;

        [Required]
        [StringLength(50)]
        public string RefundId { get; set; } = string.Empty;

        [Required]
        public decimal Amount { get; set; }

        [Required]
        [StringLength(3)]
        public string Currency { get; set; } = "INR";

        [Required]
        public RefundStatus Status { get; set; } = RefundStatus.Pending;

        [Required]
        [StringLength(500)]
        public string Reason { get; set; } = string.Empty;

        [StringLength(100)]
        public string? GatewayRefundId { get; set; }

        [StringLength(500)]
        public string? GatewayResponse { get; set; }

        [Required]
        public Guid RequestedBy { get; set; } // User who requested refund

        [StringLength(100)]
        public string RequestedByType { get; set; } = "User"; // User, Admin, System

        public DateTime RequestedAt { get; set; } = DateTime.UtcNow;

        public Guid? ProcessedBy { get; set; } // Admin who processed
        public DateTime? ProcessedAt { get; set; }

        public DateTime? CompletedAt { get; set; }

        [StringLength(10)]
        public string? FailureCode { get; set; }

        [StringLength(500)]
        public string? FailureReason { get; set; }

        [StringLength(1000)]
        public string? Notes { get; set; }

        [StringLength(1000)]
        public string? InternalNotes { get; set; } // Admin notes

        public bool IsAutoRefund { get; set; } = false; // Automatic vs manual refund

        // Refund timeline
        public int EstimatedDays { get; set; } = 7; // Expected refund timeline
        public DateTime? ExpectedCompletionDate { get; set; }

        // Refund method
        [StringLength(50)]
        public string? RefundMethod { get; set; } // Same as payment, Bank transfer, etc.

        [StringLength(100)]
        public string? RefundAccount { get; set; } // Account details for refund

        // Tax implications
        public decimal? TaxRefundAmount { get; set; }
        public decimal? ServiceFeeRefund { get; set; }
    }
}