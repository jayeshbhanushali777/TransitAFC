using Microsoft.EntityFrameworkCore;
using NetTopologySuite;
using NetTopologySuite.IO;
using TransitAFC.Services.Route.Core.Models;

namespace TransitAFC.Services.Route.Infrastructure
{
    public class RouteDbContext : DbContext
    {
        public RouteDbContext(DbContextOptions<RouteDbContext> options) : base(options)
        {
        }

        public DbSet<TransportMode> TransportModes { get; set; }
        public DbSet<Station> Stations { get; set; }
        public DbSet<Core.Models.Route> Routes { get; set; }
        public DbSet<RouteStation> RouteStations { get; set; }
        public DbSet<Schedule> Schedules { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure TransportMode entity
            modelBuilder.Entity<TransportMode>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.Code).IsUnique();
                entity.Property(e => e.Name).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Code).IsRequired().HasMaxLength(10);
                entity.Property(e => e.BaseFare).HasPrecision(10, 2);
                entity.Property(e => e.FarePerKm).HasPrecision(10, 2);
            });

            // Configure Station entity
            modelBuilder.Entity<Station>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.Code).IsUnique();
                entity.HasIndex(e => e.Location).HasMethod("GIST");

                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Code).IsRequired().HasMaxLength(20);
                entity.Property(e => e.City).IsRequired().HasMaxLength(100);
                entity.Property(e => e.State).IsRequired().HasMaxLength(100);

                // Configure geographic data
                entity.Property(e => e.Location)
                    .HasColumnType("geography (point)")
                    .IsRequired();

                entity.HasOne(e => e.TransportMode)
                    .WithMany(tm => tm.Stations)
                    .HasForeignKey(e => e.TransportModeId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Configure Route entity
            modelBuilder.Entity<Core.Models.Route>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.Code).IsUnique();
                entity.HasIndex(e => new { e.StartStationId, e.EndStationId });

                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Code).IsRequired().HasMaxLength(20);
                entity.Property(e => e.TotalDistance).HasPrecision(10, 2);
                entity.Property(e => e.BaseFare).HasPrecision(10, 2);

                entity.HasOne(e => e.TransportMode)
                    .WithMany(tm => tm.Routes)
                    .HasForeignKey(e => e.TransportModeId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.StartStation)
                    .WithMany()
                    .HasForeignKey(e => e.StartStationId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.EndStation)
                    .WithMany()
                    .HasForeignKey(e => e.EndStationId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Configure RouteStation entity
            modelBuilder.Entity<RouteStation>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => new { e.RouteId, e.StationOrder }).IsUnique();
                entity.HasIndex(e => new { e.RouteId, e.StationId }).IsUnique();

                entity.Property(e => e.DistanceFromStart).HasPrecision(10, 2);
                entity.Property(e => e.FareFromStart).HasPrecision(10, 2);

                entity.HasOne(e => e.Route)
                    .WithMany(r => r.RouteStations)
                    .HasForeignKey(e => e.RouteId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.Station)
                    .WithMany(s => s.RouteStations)
                    .HasForeignKey(e => e.StationId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Configure Schedule entity
            modelBuilder.Entity<Schedule>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => new { e.RouteId, e.StationId, e.DepartureTime });

                entity.HasOne(e => e.Route)
                    .WithMany(r => r.Schedules)
                    .HasForeignKey(e => e.RouteId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.Station)
                    .WithMany(s => s.Schedules)
                    .HasForeignKey(e => e.StationId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Seed data
            SeedData(modelBuilder);
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseNpgsql(options => options.UseNetTopologySuite());
            }
        }

        private static void SeedData(ModelBuilder modelBuilder)
        {
            // Seed Transport Modes
            var busMode = new TransportMode
            {
                Id = Guid.Parse("11111111-1111-1111-1111-111111111111"),
                Name = "Bus",
                Code = "BUS",
                Description = "Public bus transportation",
                BaseFare = 5.00m,
                FarePerKm = 2.00m,
                MaxCapacity = 50,
                IsRealTimeEnabled = true,
                Color = "#FF6B35"
            };

            var metroMode = new TransportMode
            {
                Id = Guid.Parse("22222222-2222-2222-2222-222222222222"),
                Name = "Metro",
                Code = "MET",
                Description = "Metro rail system",
                BaseFare = 10.00m,
                FarePerKm = 3.00m,
                MaxCapacity = 200,
                IsRealTimeEnabled = true,
                Color = "#004E89"
            };

            var trainMode = new TransportMode
            {
                Id = Guid.Parse("33333333-3333-3333-3333-333333333333"),
                Name = "Train",
                Code = "TRN",
                Description = "Railway transportation",
                BaseFare = 15.00m,
                FarePerKm = 1.50m,
                MaxCapacity = 500,
                IsRealTimeEnabled = false,
                Color = "#009639"
            };

            modelBuilder.Entity<TransportMode>().HasData(busMode, metroMode, trainMode);


            var wktReader = new WKTReader(NtsGeometryServices.Instance);

            // Create static Point geometries using WKT
            var churchgateLocation = wktReader.Read("POINT(72.8264 18.9322)");
            var marineLinesLocation = wktReader.Read("POINT(72.8233 18.9467)");

            // Seed sample stations with static geometry objects
            modelBuilder.Entity<Station>().HasData(
                new Station
                {
                    Id = Guid.Parse("44444444-4444-4444-4444-444444444444"),
                    Name = "Churchgate",
                    Code = "CG",
                    Address = "Veer Nariman Road, Fort",
                    City = "Mumbai",
                    State = "Maharashtra",
                    PinCode = "400001",
                    Latitude = 18.9322,
                    Longitude = 72.8264,
                    Location = (NetTopologySuite.Geometries.Point)churchgateLocation, // Static geometry object
                    TransportModeId = Guid.Parse("33333333-3333-3333-3333-333333333333"), // Train
                    StationType = "Terminal",
                    HasWheelchairAccess = true,
                    HasParking = true,
                    HasWiFi = true,
                    HasRestroom = true,
                    PlatformCount = 6,
                    Amenities = "[\"WiFi\",\"Restroom\",\"ATM\",\"Food Court\"]"
                },
                new Station
                {
                    Id = Guid.Parse("55555555-5555-5555-5555-555555555555"),
                    Name = "Marine Lines",
                    Code = "ML",
                    Address = "Marine Drive",
                    City = "Mumbai",
                    State = "Maharashtra",
                    PinCode = "400002",
                    Latitude = 18.9467,
                    Longitude = 72.8233,
                    Location = (NetTopologySuite.Geometries.Point)marineLinesLocation, // Static geometry object
                    TransportModeId = Guid.Parse("33333333-3333-3333-3333-333333333333"), // Train
                    StationType = "Regular",
                    HasWheelchairAccess = true,
                    HasParking = false,
                    HasWiFi = true,
                    HasRestroom = true,
                    PlatformCount = 4,
                    Amenities = "[\"WiFi\",\"Restroom\",\"ATM\",\"Food Court\"]"
                }
            );
        }
    }
}