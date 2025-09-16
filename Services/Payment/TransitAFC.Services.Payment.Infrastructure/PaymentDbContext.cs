using Microsoft.EntityFrameworkCore;
using TransitAFC.Services.Payment.Core.Models;

namespace TransitAFC.Services.Payment.Infrastructure
{
    public class PaymentDbContext : DbContext
    {
        public PaymentDbContext(DbContextOptions<PaymentDbContext> options) : base(options)
        {
        }

        public DbSet<Core.Models.Payment> Payments { get; set; }
        public DbSet<PaymentTransaction> PaymentTransactions { get; set; }
        public DbSet<PaymentRefund> PaymentRefunds { get; set; }
        public DbSet<PaymentHistory> PaymentHistory { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure Payment entity
            modelBuilder.Entity<Core.Models.Payment>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.PaymentId).IsUnique();
                entity.HasIndex(e => e.UserId);
                entity.HasIndex(e => e.BookingId);
                entity.HasIndex(e => e.Status);
                entity.HasIndex(e => e.Method);
                entity.HasIndex(e => e.Gateway);
                entity.HasIndex(e => e.GatewayPaymentId);
                entity.HasIndex(e => e.TransactionId);
                entity.HasIndex(e => new { e.UserId, e.Status });
                entity.HasIndex(e => new { e.Gateway, e.Status });
                entity.HasIndex(e => new { e.CreatedAt, e.Status });

                entity.Property(e => e.PaymentId).IsRequired().HasMaxLength(50);
                entity.Property(e => e.BookingNumber).IsRequired().HasMaxLength(20);
                entity.Property(e => e.Currency).IsRequired().HasMaxLength(3).HasDefaultValue("INR");
                entity.Property(e => e.CustomerEmail).IsRequired().HasMaxLength(100);
                entity.Property(e => e.CustomerPhone).IsRequired().HasMaxLength(15);
                entity.Property(e => e.CustomerName).HasMaxLength(100);

                entity.Property(e => e.Amount).HasPrecision(12, 2);
                entity.Property(e => e.TaxAmount).HasPrecision(10, 2);
                entity.Property(e => e.ServiceFee).HasPrecision(10, 2);
                entity.Property(e => e.GatewayFee).HasPrecision(10, 2);
                entity.Property(e => e.DiscountAmount).HasPrecision(10, 2);
                entity.Property(e => e.TotalAmount).HasPrecision(12, 2);
                entity.Property(e => e.RefundedAmount).HasPrecision(12, 2).HasDefaultValue(0);

                entity.Property(e => e.Status).HasConversion<string>();
                entity.Property(e => e.Method).HasConversion<string>();
                entity.Property(e => e.Gateway).HasConversion<string>();
                entity.Property(e => e.Mode).HasConversion<string>();

                entity.Property(e => e.GatewayPaymentId).HasMaxLength(100);
                entity.Property(e => e.GatewayOrderId).HasMaxLength(100);
                entity.Property(e => e.TransactionId).HasMaxLength(100);
                entity.Property(e => e.UpiId).HasMaxLength(50);
                entity.Property(e => e.CardLast4Digits).HasMaxLength(20);
                entity.Property(e => e.CardType).HasMaxLength(50);
                entity.Property(e => e.WalletType).HasMaxLength(50);

                entity.Property(e => e.FailureCode).HasMaxLength(10);
                entity.Property(e => e.FailureReason).HasMaxLength(500);
                entity.Property(e => e.IpAddress).HasMaxLength(50);
                entity.Property(e => e.RiskCategory).HasMaxLength(20);

                entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
                entity.Property(e => e.IsActive).HasDefaultValue(true);
                entity.Property(e => e.IsDeleted).HasDefaultValue(false);
            });

            // Configure PaymentTransaction entity
            modelBuilder.Entity<PaymentTransaction>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.PaymentId);
                entity.HasIndex(e => e.TransactionId);
                entity.HasIndex(e => e.Type);
                entity.HasIndex(e => e.Status);
                entity.HasIndex(e => new { e.PaymentId, e.Type });

                entity.Property(e => e.TransactionId).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Currency).IsRequired().HasMaxLength(3).HasDefaultValue("INR");
                entity.Property(e => e.Amount).HasPrecision(12, 2);
                entity.Property(e => e.ReconciledAmount).HasPrecision(12, 2);

                entity.Property(e => e.Type).HasConversion<string>();
                entity.Property(e => e.Status).HasConversion<string>();

                entity.Property(e => e.GatewayTransactionId).HasMaxLength(100);
                entity.Property(e => e.ResponseCode).HasMaxLength(10);
                entity.Property(e => e.ResponseMessage).HasMaxLength(500);
                entity.Property(e => e.ProcessedBy).HasMaxLength(50);
                entity.Property(e => e.SettlementId).HasMaxLength(100);

                entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
                entity.Property(e => e.IsActive).HasDefaultValue(true);
                entity.Property(e => e.IsDeleted).HasDefaultValue(false);

                entity.HasOne(e => e.Payment)
                    .WithMany(p => p.Transactions)
                    .HasForeignKey(e => e.PaymentId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Configure PaymentRefund entity
            modelBuilder.Entity<PaymentRefund>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.PaymentId);
                entity.HasIndex(e => e.RefundId);
                entity.HasIndex(e => e.Status);
                entity.HasIndex(e => e.RequestedBy);
                entity.HasIndex(e => new { e.PaymentId, e.Status });

                entity.Property(e => e.RefundId).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Currency).IsRequired().HasMaxLength(3).HasDefaultValue("INR");
                entity.Property(e => e.Amount).HasPrecision(12, 2);
                entity.Property(e => e.TaxRefundAmount).HasPrecision(10, 2);
                entity.Property(e => e.ServiceFeeRefund).HasPrecision(10, 2);

                entity.Property(e => e.Status).HasConversion<string>();
                entity.Property(e => e.Reason).IsRequired().HasMaxLength(500);
                entity.Property(e => e.RequestedByType).HasMaxLength(100).HasDefaultValue("User");
                entity.Property(e => e.GatewayRefundId).HasMaxLength(100);
                entity.Property(e => e.FailureCode).HasMaxLength(10);
                entity.Property(e => e.FailureReason).HasMaxLength(500);
                entity.Property(e => e.RefundMethod).HasMaxLength(50);
                entity.Property(e => e.RefundAccount).HasMaxLength(100);

                entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
                entity.Property(e => e.IsActive).HasDefaultValue(true);
                entity.Property(e => e.IsDeleted).HasDefaultValue(false);

                entity.HasOne(e => e.Payment)
                    .WithMany(p => p.Refunds)
                    .HasForeignKey(e => e.PaymentId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Configure PaymentHistory entity
            modelBuilder.Entity<PaymentHistory>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.PaymentId);
                entity.HasIndex(e => e.ActionTime);
                entity.HasIndex(e => new { e.PaymentId, e.ActionTime });

                entity.Property(e => e.FromStatus).HasConversion<string>();
                entity.Property(e => e.ToStatus).HasConversion<string>();
                entity.Property(e => e.Action).IsRequired().HasMaxLength(100);
                entity.Property(e => e.ActionByType).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Reason).HasMaxLength(500);
                entity.Property(e => e.Notes).HasMaxLength(1000);
                entity.Property(e => e.IpAddress).HasMaxLength(50);
                entity.Property(e => e.UserAgent).HasMaxLength(200);
                entity.Property(e => e.GatewayReference).HasMaxLength(100);

                entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
                entity.Property(e => e.IsActive).HasDefaultValue(true);
                entity.Property(e => e.IsDeleted).HasDefaultValue(false);

                entity.HasOne(e => e.Payment)
                    .WithMany(p => p.PaymentHistory)
                    .HasForeignKey(e => e.PaymentId)
                    .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}