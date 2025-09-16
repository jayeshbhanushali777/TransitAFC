using System.ComponentModel.DataAnnotations;
using TransitAFC.Shared.Common.Models;

namespace TransitAFC.Services.Payment.Core.Models
{
    public class PaymentTransaction : BaseEntity
    {
        [Required]
        public Guid PaymentId { get; set; }
        public Payment Payment { get; set; } = null!;

        [Required]
        [StringLength(50)]
        public string TransactionId { get; set; } = string.Empty;

        [Required]
        public TransactionType Type { get; set; }

        [Required]
        public decimal Amount { get; set; }

        [Required]
        [StringLength(3)]
        public string Currency { get; set; } = "INR";

        [Required]
        public PaymentStatus Status { get; set; }

        [StringLength(100)]
        public string? GatewayTransactionId { get; set; }

        [StringLength(500)]
        public string? GatewayResponse { get; set; }

        [StringLength(10)]
        public string? ResponseCode { get; set; }

        [StringLength(500)]
        public string? ResponseMessage { get; set; }

        public DateTime ProcessedAt { get; set; } = DateTime.UtcNow;

        [StringLength(50)]
        public string? ProcessedBy { get; set; } // System, Admin, etc.

        [StringLength(1000)]
        public string? Notes { get; set; }

        [StringLength(2000)]
        public string? RawResponse { get; set; } // Complete gateway response

        // Reconciliation fields
        public bool IsReconciled { get; set; } = false;
        public DateTime? ReconciledAt { get; set; }
        public decimal? ReconciledAmount { get; set; }

        [StringLength(100)]
        public string? SettlementId { get; set; }
        public DateTime? SettledAt { get; set; }
    }
}