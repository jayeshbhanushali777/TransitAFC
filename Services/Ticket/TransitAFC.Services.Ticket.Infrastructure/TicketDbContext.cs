using Microsoft.EntityFrameworkCore;
using TransitAFC.Services.Ticket.Core.Models;

namespace TransitAFC.Services.Ticket.Infrastructure
{
    public class TicketDbContext : DbContext
    {
        public TicketDbContext(DbContextOptions<TicketDbContext> options) : base(options)
        {
        }

        public DbSet<Core.Models.Ticket> Tickets { get; set; }
        public DbSet<TicketValidation> TicketValidations { get; set; }
        public DbSet<TicketQRCode> TicketQRCodes { get; set; }
        public DbSet<TicketHistory> TicketHistory { get; set; }
        public DbSet<TicketTransfer> TicketTransfers { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure Ticket entity
            modelBuilder.Entity<Core.Models.Ticket>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.TicketNumber).IsUnique();
                entity.HasIndex(e => e.UserId);
                entity.HasIndex(e => e.BookingId);
                entity.HasIndex(e => e.PaymentId);
                entity.HasIndex(e => e.Status);
                entity.HasIndex(e => e.Type);
                entity.HasIndex(e => e.TransportMode);
                entity.HasIndex(e => e.QRCodeHash);
                entity.HasIndex(e => new { e.UserId, e.Status });
                entity.HasIndex(e => new { e.ValidFrom, e.ValidUntil });
                entity.HasIndex(e => new { e.SourceStationId, e.DestinationStationId });
                entity.HasIndex(e => new { e.CreatedAt, e.Status });

                entity.Property(e => e.TicketNumber).IsRequired().HasMaxLength(50);
                entity.Property(e => e.BookingNumber).IsRequired().HasMaxLength(20);
                entity.Property(e => e.SourceStationName).IsRequired().HasMaxLength(100);
                entity.Property(e => e.SourceStationCode).IsRequired().HasMaxLength(10);
                entity.Property(e => e.DestinationStationName).IsRequired().HasMaxLength(100);
                entity.Property(e => e.DestinationStationCode).IsRequired().HasMaxLength(10);
                entity.Property(e => e.RouteName).HasMaxLength(100);
                entity.Property(e => e.RouteCode).HasMaxLength(20);
                entity.Property(e => e.Currency).IsRequired().HasMaxLength(3).HasDefaultValue("INR");
                entity.Property(e => e.PassengerName).IsRequired().HasMaxLength(100);
                entity.Property(e => e.PassengerAge).HasMaxLength(20);
                entity.Property(e => e.PassengerType).HasMaxLength(20).HasDefaultValue("Adult");
                entity.Property(e => e.PassengerPhone).HasMaxLength(15);
                entity.Property(e => e.PassengerEmail).HasMaxLength(100);
                entity.Property(e => e.QRCodeData).IsRequired().HasMaxLength(500);
                entity.Property(e => e.QRCodeHash).HasMaxLength(100);

                entity.Property(e => e.BasePrice).HasPrecision(10, 2);
                entity.Property(e => e.DiscountAmount).HasPrecision(10, 2);
                entity.Property(e => e.TaxAmount).HasPrecision(10, 2);
                entity.Property(e => e.FinalPrice).HasPrecision(10, 2);
                entity.Property(e => e.EstimatedDuration).HasPrecision(8, 2);
                entity.Property(e => e.EstimatedDistance).HasPrecision(8, 2);
                entity.Property(e => e.TransferTimeLimit).HasPrecision(8, 2);

                entity.Property(e => e.Status).HasConversion<string>();
                entity.Property(e => e.Type).HasConversion<string>();
                entity.Property(e => e.TransportMode).HasConversion<string>();
                entity.Property(e => e.FareType).HasConversion<string>();

                entity.Property(e => e.ZoneFrom).HasMaxLength(20);
                entity.Property(e => e.ZoneTo).HasMaxLength(20);
                entity.Property(e => e.ServiceClass).HasMaxLength(20).HasDefaultValue("Standard");
                entity.Property(e => e.SeatNumber).HasMaxLength(10);
                entity.Property(e => e.CoachNumber).HasMaxLength(5);
                entity.Property(e => e.RegulatoryCode).HasMaxLength(50);
                entity.Property(e => e.OperatorCode).HasMaxLength(100);

                entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
                entity.Property(e => e.IsActive).HasDefaultValue(true);
                entity.Property(e => e.IsDeleted).HasDefaultValue(false);
                entity.Property(e => e.IsRefundable).HasDefaultValue(true);
                entity.Property(e => e.IsTransferable).HasDefaultValue(false);
                entity.Property(e => e.RequiresValidation).HasDefaultValue(true);
            });

            // Configure TicketValidation entity
            modelBuilder.Entity<TicketValidation>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.TicketId);
                entity.HasIndex(e => e.ValidationId);
                entity.HasIndex(e => e.StationId);
                entity.HasIndex(e => e.DeviceId);
                entity.HasIndex(e => e.ValidationTime);
                entity.HasIndex(e => e.ValidationResult);
                entity.HasIndex(e => new { e.TicketId, e.ValidationTime });
                entity.HasIndex(e => new { e.StationId, e.ValidationTime });

                entity.Property(e => e.ValidationId).IsRequired().HasMaxLength(50);
                entity.Property(e => e.StationName).IsRequired().HasMaxLength(100);
                entity.Property(e => e.StationCode).IsRequired().HasMaxLength(10);
                entity.Property(e => e.ZoneCode).HasMaxLength(20);
                entity.Property(e => e.DeviceName).IsRequired().HasMaxLength(50);
                entity.Property(e => e.DeviceType).HasMaxLength(20);
                entity.Property(e => e.DeviceSerialNumber).HasMaxLength(50);
                entity.Property(e => e.OperatorName).HasMaxLength(100);
                entity.Property(e => e.OperatorBadgeNumber).HasMaxLength(20);
                entity.Property(e => e.ValidationMethod).HasMaxLength(100);
                entity.Property(e => e.Direction).HasMaxLength(20);
                entity.Property(e => e.Platform).HasMaxLength(50);
                entity.Property(e => e.Gate).HasMaxLength(20);
                entity.Property(e => e.VehicleNumber).HasMaxLength(20);
                entity.Property(e => e.RouteNumber).HasMaxLength(50);
                entity.Property(e => e.SystemVersion).HasMaxLength(50);
                entity.Property(e => e.TransactionId).HasMaxLength(50);

                entity.Property(e => e.ValidationType).HasConversion<string>();
                entity.Property(e => e.ValidationResult).HasConversion<string>();

                entity.Property(e => e.FareDeducted).HasPrecision(10, 2);
                entity.Property(e => e.BalanceAfter).HasPrecision(10, 2);
                entity.Property(e => e.Latitude).HasPrecision(10, 7);
                entity.Property(e => e.Longitude).HasPrecision(10, 7);

                entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
                entity.Property(e => e.IsActive).HasDefaultValue(true);
                entity.Property(e => e.IsDeleted).HasDefaultValue(false);

                entity.HasOne(e => e.Ticket)
                    .WithMany(t => t.Validations)
                    .HasForeignKey(e => e.TicketId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Configure TicketQRCode entity
            modelBuilder.Entity<TicketQRCode>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.TicketId);
                entity.HasIndex(e => e.QRCodeId);
                entity.HasIndex(e => e.QRCodeHash);
                entity.HasIndex(e => e.Status);
                entity.HasIndex(e => e.ExpiresAt);
                entity.HasIndex(e => new { e.TicketId, e.Status });

                entity.Property(e => e.QRCodeId).IsRequired().HasMaxLength(100);
                entity.Property(e => e.QRCodeData).IsRequired().HasMaxLength(2000);
                entity.Property(e => e.QRCodeHash).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Format).HasMaxLength(20).HasDefaultValue("PNG");
                entity.Property(e => e.ErrorCorrectionLevel).HasMaxLength(20).HasDefaultValue("M");
                entity.Property(e => e.EncryptionKey).HasMaxLength(100);
                entity.Property(e => e.EncryptionAlgorithm).HasMaxLength(50).HasDefaultValue("AES256");
                entity.Property(e => e.Salt).HasMaxLength(32);
                entity.Property(e => e.LastScannedBy).HasMaxLength(50);
                entity.Property(e => e.LastScannedDevice).HasMaxLength(100);
                entity.Property(e => e.RegenerationReason).HasMaxLength(200);
                entity.Property(e => e.DisplayText).HasMaxLength(200);

                entity.Property(e => e.Status).HasConversion<string>();

                entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
                entity.Property(e => e.IsActive).HasDefaultValue(true);
                entity.Property(e => e.IsDeleted).HasDefaultValue(false);

                entity.HasOne(e => e.Ticket)
                    .WithMany(t => t.QRCodes)
                    .HasForeignKey(e => e.TicketId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Configure TicketHistory entity
            modelBuilder.Entity<TicketHistory>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.TicketId);
                entity.HasIndex(e => e.ActionTime);
                entity.HasIndex(e => e.ActionBy);
                entity.HasIndex(e => new { e.TicketId, e.ActionTime });

                entity.Property(e => e.FromStatus).HasConversion<string>();
                entity.Property(e => e.ToStatus).HasConversion<string>();
                entity.Property(e => e.Action).IsRequired().HasMaxLength(100);
                entity.Property(e => e.ActionByType).IsRequired().HasMaxLength(100);
                entity.Property(e => e.ActionByName).HasMaxLength(100);
                entity.Property(e => e.Reason).HasMaxLength(500);
                entity.Property(e => e.Notes).HasMaxLength(1000);
                entity.Property(e => e.IpAddress).HasMaxLength(50);
                entity.Property(e => e.UserAgent).HasMaxLength(200);
                entity.Property(e => e.StationName).HasMaxLength(100);
                entity.Property(e => e.DeviceName).HasMaxLength(50);
                entity.Property(e => e.ReferenceId).HasMaxLength(100);

                entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
                entity.Property(e => e.IsActive).HasDefaultValue(true);
                entity.Property(e => e.IsDeleted).HasDefaultValue(false);

                entity.HasOne(e => e.Ticket)
                    .WithMany(t => t.TicketHistory)
                    .HasForeignKey(e => e.TicketId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Configure TicketTransfer entity
            modelBuilder.Entity<TicketTransfer>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.TicketId);
                entity.HasIndex(e => e.TransferId);
                entity.HasIndex(e => e.FromStationId);
                entity.HasIndex(e => e.ToStationId);
                entity.HasIndex(e => e.ExitTime);
                entity.HasIndex(e => e.EntryTime);
                entity.HasIndex(e => new { e.TicketId, e.TransferSequence });

                entity.Property(e => e.TransferId).IsRequired().HasMaxLength(50);
                entity.Property(e => e.FromStationName).IsRequired().HasMaxLength(100);
                entity.Property(e => e.FromStationCode).IsRequired().HasMaxLength(10);
                entity.Property(e => e.ToStationName).IsRequired().HasMaxLength(100);
                entity.Property(e => e.ToStationCode).IsRequired().HasMaxLength(10);
                entity.Property(e => e.FromRouteNumber).HasMaxLength(50);
                entity.Property(e => e.ToRouteNumber).HasMaxLength(50);
                entity.Property(e => e.FromVehicleNumber).HasMaxLength(20);
                entity.Property(e => e.ToVehicleNumber).HasMaxLength(20);
                entity.Property(e => e.TransferCode).HasMaxLength(50);
                entity.Property(e => e.TransferReason).HasMaxLength(200);
                entity.Property(e => e.AuthorizedByName).HasMaxLength(100);
                entity.Property(e => e.AuthorizedByBadge).HasMaxLength(20);

                entity.Property(e => e.FromTransportMode).HasConversion<string>();
                entity.Property(e => e.ToTransportMode).HasConversion<string>();

                entity.Property(e => e.TransferTime).HasPrecision(8, 2);
                entity.Property(e => e.WalkingDistance).HasPrecision(8, 2);
                entity.Property(e => e.TransferFee).HasPrecision(10, 2);
                entity.Property(e => e.AdditionalFare).HasPrecision(10, 2);

                entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
                entity.Property(e => e.IsActive).HasDefaultValue(true);
                entity.Property(e => e.IsDeleted).HasDefaultValue(false);

                entity.HasOne(e => e.Ticket)
                    .WithMany(t => t.Transfers)
                    .HasForeignKey(e => e.TicketId)
                    .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}