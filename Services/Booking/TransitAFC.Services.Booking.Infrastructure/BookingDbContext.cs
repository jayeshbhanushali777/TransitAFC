using Microsoft.EntityFrameworkCore;
using TransitAFC.Services.Booking.Core.Models;

namespace TransitAFC.Services.Booking.Infrastructure
{
    public class BookingDbContext : DbContext
    {
        public BookingDbContext(DbContextOptions<BookingDbContext> options) : base(options)
        {
        }

        public DbSet<Core.Models.Booking> Bookings { get; set; }
        public DbSet<BookingPassenger> BookingPassengers { get; set; }
        public DbSet<BookingHistory> BookingHistory { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure Booking entity
            modelBuilder.Entity<Core.Models.Booking>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.BookingNumber).IsUnique();
                entity.HasIndex(e => e.UserId);
                entity.HasIndex(e => e.RouteId);
                entity.HasIndex(e => e.Status);
                entity.HasIndex(e => e.DepartureTime);
                entity.HasIndex(e => new { e.UserId, e.Status });
                entity.HasIndex(e => new { e.RouteId, e.DepartureTime });

                entity.Property(e => e.BookingNumber).IsRequired().HasMaxLength(20);
                entity.Property(e => e.RouteCode).IsRequired().HasMaxLength(20);
                entity.Property(e => e.RouteName).IsRequired().HasMaxLength(100);
                entity.Property(e => e.SourceStationName).IsRequired().HasMaxLength(100);
                entity.Property(e => e.DestinationStationName).IsRequired().HasMaxLength(100);
                entity.Property(e => e.ContactEmail).IsRequired().HasMaxLength(100);
                entity.Property(e => e.ContactPhone).IsRequired().HasMaxLength(15);
                entity.Property(e => e.Currency).HasMaxLength(50).HasDefaultValue("INR");
                entity.Property(e => e.BookingSource).HasMaxLength(50).HasDefaultValue("WebApp");

                entity.Property(e => e.TotalFare).HasPrecision(10, 2);
                entity.Property(e => e.DiscountAmount).HasPrecision(10, 2);
                entity.Property(e => e.TaxAmount).HasPrecision(10, 2);
                entity.Property(e => e.FinalAmount).HasPrecision(10, 2);

                entity.Property(e => e.Status).HasConversion<string>();
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
                entity.Property(e => e.IsActive).HasDefaultValue(true);
                entity.Property(e => e.IsDeleted).HasDefaultValue(false);
            });

            // Configure BookingPassenger entity
            modelBuilder.Entity<BookingPassenger>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.BookingId);
                entity.HasIndex(e => new { e.BookingId, e.IsPrimaryPassenger });

                entity.Property(e => e.FirstName).IsRequired().HasMaxLength(100);
                entity.Property(e => e.LastName).IsRequired().HasMaxLength(100);
                entity.Property(e => e.PassengerType).HasConversion<string>();
                entity.Property(e => e.SeatType).HasConversion<string>();
                entity.Property(e => e.Gender).HasMaxLength(10);
                entity.Property(e => e.SeatNumber).HasMaxLength(50);
                entity.Property(e => e.IdentityType).HasMaxLength(50);
                entity.Property(e => e.IdentityNumber).HasMaxLength(50);
                entity.Property(e => e.ContactPhone).HasMaxLength(15);
                entity.Property(e => e.ContactEmail).HasMaxLength(100);
                entity.Property(e => e.TicketNumber).HasMaxLength(20);

                entity.Property(e => e.Fare).HasPrecision(10, 2);
                entity.Property(e => e.DiscountAmount).HasPrecision(10, 2);
                entity.Property(e => e.FinalFare).HasPrecision(10, 2);

                entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
                entity.Property(e => e.IsActive).HasDefaultValue(true);
                entity.Property(e => e.IsDeleted).HasDefaultValue(false);

                entity.HasOne(e => e.Booking)
                    .WithMany(b => b.Passengers)
                    .HasForeignKey(e => e.BookingId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Configure BookingHistory entity
            modelBuilder.Entity<BookingHistory>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.BookingId);
                entity.HasIndex(e => e.ActionTime);
                entity.HasIndex(e => new { e.BookingId, e.ActionTime });

                entity.Property(e => e.FromStatus).HasConversion<string>();
                entity.Property(e => e.ToStatus).HasConversion<string>();
                entity.Property(e => e.Action).IsRequired().HasMaxLength(100);
                entity.Property(e => e.ActionByType).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Reason).HasMaxLength(500);
                entity.Property(e => e.Notes).HasMaxLength(1000);
                entity.Property(e => e.IpAddress).HasMaxLength(50);
                entity.Property(e => e.UserAgent).HasMaxLength(100);

                entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
                entity.Property(e => e.IsActive).HasDefaultValue(true);
                entity.Property(e => e.IsDeleted).HasDefaultValue(false);

                entity.HasOne(e => e.Booking)
                    .WithMany(b => b.BookingHistory)
                    .HasForeignKey(e => e.BookingId)
                    .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}