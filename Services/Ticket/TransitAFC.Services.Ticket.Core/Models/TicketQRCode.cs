using System.ComponentModel.DataAnnotations;
using TransitAFC.Shared.Common.Models;

namespace TransitAFC.Services.Ticket.Core.Models
{
    public class TicketQRCode : BaseEntity
    {
        [Required]
        public Guid TicketId { get; set; }
        public Ticket Ticket { get; set; } = null!;

        [Required]
        [StringLength(100)]
        public string QRCodeId { get; set; } = string.Empty; // Unique QR code identifier

        [Required]
        [StringLength(2000)]
        public string QRCodeData { get; set; } = string.Empty; // Encrypted QR code data

        [Required]
        [StringLength(100)]
        public string QRCodeHash { get; set; } = string.Empty; // Hash for validation

        [Required]
        public byte[] QRCodeImage { get; set; } = Array.Empty<byte>(); // QR code image

        [Required]
        public QRCodeStatus Status { get; set; } = QRCodeStatus.Active;

        [Required]
        public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;

        [Required]
        public DateTime ExpiresAt { get; set; }

        public DateTime? UsedAt { get; set; }
        public DateTime? InvalidatedAt { get; set; }

        // QR Code Specifications
        public int Size { get; set; } = 300; // Pixel size
        public int Version { get; set; } = 1; // QR code version number

        [StringLength(20)]
        public string Format { get; set; } = "PNG"; // Image format

        [StringLength(20)]
        public string ErrorCorrectionLevel { get; set; } = "M"; // L, M, Q, H

        // Security
        [StringLength(100)]
        public string EncryptionKey { get; set; } = string.Empty;

        [StringLength(50)]
        public string EncryptionAlgorithm { get; set; } = "AES256";

        [StringLength(32)]
        public string Salt { get; set; } = string.Empty;

        // Usage Tracking
        public int ScanCount { get; set; } = 0;
        public DateTime? LastScannedAt { get; set; }

        [StringLength(50)]
        public string? LastScannedBy { get; set; }

        [StringLength(100)]
        public string? LastScannedDevice { get; set; }

        // Regeneration Info
        public bool IsRegenerated { get; set; } = false;
        public Guid? PreviousQRCodeId { get; set; }
        public Guid? NextQRCodeId { get; set; }

        [StringLength(200)]
        public string? RegenerationReason { get; set; }

        // Display Information
        [StringLength(200)]
        public string? DisplayText { get; set; } // Text to display with QR code

        [StringLength(500)]
        public string? Instructions { get; set; } // Instructions for scanning

        // Metadata
        [StringLength(1000)]
        public string? AdditionalData { get; set; } // JSON string for extra data
    }
}