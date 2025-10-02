using Microsoft.EntityFrameworkCore;
using NetTopologySuite;
using NetTopologySuite.IO;
using NetTopologySuite.Geometries;
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
            // Use a fixed date instead of DateTime.UtcNow
            var seedDate = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc);

            // Seed Transport Modes with static values
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
                Color = "#FF6B35",
                CreatedAt = seedDate,
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
                Color = "#004E89",
                CreatedAt = seedDate,
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
                Color = "#009639",
                CreatedAt = seedDate,
            };

            modelBuilder.Entity<TransportMode>().HasData(busMode, metroMode, trainMode);

            // Create static geometry objects
            var geometryFactory = NtsGeometryServices.Instance.CreateGeometryFactory(srid: 4326);

            // Train Stations (Western Line)
            var churchgateLocation = geometryFactory.CreatePoint(new Coordinate(72.8264, 18.9322));
            var marineLinesLocation = geometryFactory.CreatePoint(new Coordinate(72.8233, 18.9467));
            var charniRoadLocation = geometryFactory.CreatePoint(new Coordinate(72.8202, 18.9536));
            var grantRoadLocation = geometryFactory.CreatePoint(new Coordinate(72.8170, 18.9629));
            var mumbaiCentralLocation = geometryFactory.CreatePoint(new Coordinate(72.8195, 18.9690));
            var maladhillLocation = geometryFactory.CreatePoint(new Coordinate(72.8485, 19.1840));

            // Metro Stations
            var ghatkoparLocation = geometryFactory.CreatePoint(new Coordinate(72.9081, 19.0863));
            var andheriLocation = geometryFactory.CreatePoint(new Coordinate(72.8397, 19.1136));
            var versovaLocation = geometryFactory.CreatePoint(new Coordinate(72.8154, 19.1313));
            var azadNagarLocation = geometryFactory.CreatePoint(new Coordinate(72.8697, 19.1234));

            // Bus Stations
            var bandraLocation = geometryFactory.CreatePoint(new Coordinate(72.8400, 19.0544));
            var juhuLocation = geometryFactory.CreatePoint(new Coordinate(72.8267, 19.1075));
            var powaiLocation = geometryFactory.CreatePoint(new Coordinate(72.9050, 19.1268));
            var bkcLocation = geometryFactory.CreatePoint(new Coordinate(72.8328, 19.0705));

            // Seed Stations
            var stations = new[]
            {
        // Train Stations
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
            Location = churchgateLocation,
            TransportModeId = Guid.Parse("33333333-3333-3333-3333-333333333333"),
            StationType = "Terminal",
            HasWheelchairAccess = true,
            HasParking = true,
            HasWiFi = true,
            HasRestroom = true,
            PlatformCount = 6,
            Amenities = "[\"WiFi\",\"Restroom\",\"ATM\",\"Food Court\"]",
            CreatedAt = seedDate,
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
            Location = marineLinesLocation,
            TransportModeId = Guid.Parse("33333333-3333-3333-3333-333333333333"),
            StationType = "Regular",
            HasWheelchairAccess = true,
            HasParking = false,
            HasWiFi = true,
            HasRestroom = true,
            PlatformCount = 4,
            Amenities = "[\"WiFi\",\"Restroom\",\"ATM\"]",
            CreatedAt = seedDate,
        },
        new Station
        {
            Id = Guid.Parse("66666666-6666-6666-6666-666666666666"),
            Name = "Charni Road",
            Code = "CR",
            Address = "Charni Road",
            City = "Mumbai",
            State = "Maharashtra",
            PinCode = "400004",
            Latitude = 18.9536,
            Longitude = 72.8202,
            Location = charniRoadLocation,
            TransportModeId = Guid.Parse("33333333-3333-3333-3333-333333333333"),
            StationType = "Regular",
            HasWheelchairAccess = true,
            HasParking = false,
            HasWiFi = false,
            HasRestroom = true,
            PlatformCount = 2,
            Amenities = "[\"Restroom\"]",
            CreatedAt = seedDate,
        },
        new Station
        {
            Id = Guid.Parse("77777777-7777-7777-7777-777777777777"),
            Name = "Grant Road",
            Code = "GR",
            Address = "Grant Road",
            City = "Mumbai",
            State = "Maharashtra",
            PinCode = "400007",
            Latitude = 18.9629,
            Longitude = 72.8170,
            Location = grantRoadLocation,
            TransportModeId = Guid.Parse("33333333-3333-3333-3333-333333333333"),
            StationType = "Regular",
            HasWheelchairAccess = false,
            HasParking = false,
            HasWiFi = false,
            HasRestroom = true,
            PlatformCount = 2,
            Amenities = "[\"Restroom\"]",
            CreatedAt = seedDate,
        },
        new Station
        {
            Id = Guid.Parse("88888888-8888-8888-8888-888888888888"),
            Name = "Mumbai Central",
            Code = "BC",
            Address = "Mumbai Central",
            City = "Mumbai",
            State = "Maharashtra",
            PinCode = "400008",
            Latitude = 18.9690,
            Longitude = 72.8195,
            Location = mumbaiCentralLocation,
            TransportModeId = Guid.Parse("33333333-3333-3333-3333-333333333333"),
            StationType = "Junction",
            HasWheelchairAccess = true,
            HasParking = true,
            HasWiFi = true,
            HasRestroom = true,
            PlatformCount = 8,
            Amenities = "[\"WiFi\",\"Restroom\",\"ATM\",\"Food Court\",\"Waiting Room\"]",
            CreatedAt = seedDate,
        },
        new Station
        {
            Id = Guid.Parse("99999999-9999-9999-9999-999999999999"),
            Name = "Malad",
            Code = "MD",
            Address = "Malad West",
            City = "Mumbai",
            State = "Maharashtra",
            PinCode = "400064",
            Latitude = 19.1840,
            Longitude = 72.8485,
            Location = maladhillLocation,
            TransportModeId = Guid.Parse("33333333-3333-3333-3333-333333333333"),
            StationType = "Regular",
            HasWheelchairAccess = true,
            HasParking = true,
            HasWiFi = true,
            HasRestroom = true,
            PlatformCount = 4,
            Amenities = "[\"WiFi\",\"Restroom\",\"ATM\"]",
            CreatedAt = seedDate,
        },

        // Metro Stations
        new Station
        {
            Id = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"),
            Name = "Ghatkopar",
            Code = "GK",
            Address = "Ghatkopar East",
            City = "Mumbai",
            State = "Maharashtra",
            PinCode = "400077",
            Latitude = 19.0863,
            Longitude = 72.9081,
            Location = ghatkoparLocation,
            TransportModeId = Guid.Parse("22222222-2222-2222-2222-222222222222"),
            StationType = "Terminal",
            HasWheelchairAccess = true,
            HasParking = true,
            HasWiFi = true,
            HasRestroom = true,
            PlatformCount = 2,
            Amenities = "[\"WiFi\",\"Restroom\",\"ATM\",\"Elevator\",\"Escalator\"]",
            CreatedAt = seedDate,
        },
        new Station
        {
            Id = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"),
            Name = "Andheri",
            Code = "AD",
            Address = "Andheri East",
            City = "Mumbai",
            State = "Maharashtra",
            PinCode = "400069",
            Latitude = 19.1136,
            Longitude = 72.8397,
            Location = andheriLocation,
            TransportModeId = Guid.Parse("22222222-2222-2222-2222-222222222222"),
            StationType = "Junction",
            HasWheelchairAccess = true,
            HasParking = true,
            HasWiFi = true,
            HasRestroom = true,
            PlatformCount = 2,
            Amenities = "[\"WiFi\",\"Restroom\",\"ATM\",\"Elevator\",\"Escalator\",\"Food Court\"]",
            CreatedAt = seedDate,
        },
        new Station
        {
            Id = Guid.Parse("cccccccc-cccc-cccc-cccc-cccccccccccc"),
            Name = "Versova",
            Code = "VS",
            Address = "Versova",
            City = "Mumbai",
            State = "Maharashtra",
            PinCode = "400061",
            Latitude = 19.1313,
            Longitude = 72.8154,
            Location = versovaLocation,
            TransportModeId = Guid.Parse("22222222-2222-2222-2222-222222222222"),
            StationType = "Terminal",
            HasWheelchairAccess = true,
            HasParking = false,
            HasWiFi = true,
            HasRestroom = true,
            PlatformCount = 1,
            Amenities = "[\"WiFi\",\"Restroom\",\"Elevator\",\"Escalator\"]",
            CreatedAt = seedDate,
        },
        new Station
        {
            Id = Guid.Parse("dddddddd-dddd-dddd-dddd-dddddddddddd"),
            Name = "Azad Nagar",
            Code = "AN",
            Address = "Azad Nagar",
            City = "Mumbai",
            State = "Maharashtra",
            PinCode = "400053",
            Latitude = 19.1234,
            Longitude = 72.8697,
            Location = azadNagarLocation,
            TransportModeId = Guid.Parse("22222222-2222-2222-2222-222222222222"),
            StationType = "Regular",
            HasWheelchairAccess = true,
            HasParking = false,
            HasWiFi = true,
            HasRestroom = false,
            PlatformCount = 1,
            Amenities = "[\"WiFi\",\"Elevator\"]",
            CreatedAt = seedDate,
        },

        // Bus Stations
        new Station
        {
            Id = Guid.Parse("eeeeeeee-eeee-eeee-eeee-eeeeeeeeeeee"),
            Name = "Bandra Terminal",
            Code = "BT",
            Address = "Bandra West",
            City = "Mumbai",
            State = "Maharashtra",
            PinCode = "400050",
            Latitude = 19.0544,
            Longitude = 72.8400,
            Location = bandraLocation,
            TransportModeId = Guid.Parse("11111111-1111-1111-1111-111111111111"),
            StationType = "Terminal",
            HasWheelchairAccess = true,
            HasParking = true,
            HasWiFi = false,
            HasRestroom = true,
            PlatformCount = 12,
            Amenities = "[\"Restroom\",\"Ticket Counter\",\"Waiting Area\"]",
            CreatedAt = seedDate,
        },
        new Station
        {
            Id = Guid.Parse("ffffffff-ffff-ffff-ffff-ffffffffffff"),
            Name = "Juhu Beach",
            Code = "JB",
            Address = "Juhu",
            City = "Mumbai",
            State = "Maharashtra",
            PinCode = "400049",
            Latitude = 19.1075,
            Longitude = 72.8267,
            Location = juhuLocation,
            TransportModeId = Guid.Parse("11111111-1111-1111-1111-111111111111"),
            StationType = "Regular",
            HasWheelchairAccess = false,
            HasParking = false,
            HasWiFi = false,
            HasRestroom = false,
            PlatformCount = 2,
            Amenities = "[\"Shelter\"]",
            CreatedAt = seedDate,
        },
        new Station
        {
            Id = Guid.Parse("10101010-1010-1010-1010-101010101010"),
            Name = "Powai",
            Code = "PW",
            Address = "Powai",
            City = "Mumbai",
            State = "Maharashtra",
            PinCode = "400076",
            Latitude = 19.1268,
            Longitude = 72.9050,
            Location = powaiLocation,
            TransportModeId = Guid.Parse("11111111-1111-1111-1111-111111111111"),
            StationType = "Regular",
            HasWheelchairAccess = true,
            HasParking = true,
            HasWiFi = false,
            HasRestroom = true,
            PlatformCount = 4,
            Amenities = "[\"Restroom\",\"Ticket Counter\"]",
            CreatedAt = seedDate,
        },
        new Station
        {
            Id = Guid.Parse("20202020-2020-2020-2020-202020202020"),
            Name = "Bandra Kurla Complex",
            Code = "BKC",
            Address = "BKC",
            City = "Mumbai",
            State = "Maharashtra",
            PinCode = "400051",
            Latitude = 19.0705,
            Longitude = 72.8328,
            Location = bkcLocation,
            TransportModeId = Guid.Parse("11111111-1111-1111-1111-111111111111"),
            StationType = "Regular",
            HasWheelchairAccess = true,
            HasParking = true,
            HasWiFi = true,
            HasRestroom = true,
            PlatformCount = 6,
            Amenities = "[\"WiFi\",\"Restroom\",\"ATM\",\"Food Court\"]",
            CreatedAt = seedDate,
        }
    };

            modelBuilder.Entity<Station>().HasData(stations);

            // Seed Routes
            var routes = new[]
            {
        // Train Route: Western Line
        new Core.Models.Route
        {
            Id = Guid.Parse("30303030-3030-3030-3030-303030303030"),
            Name = "Western Line Local",
            Code = "WL01",
            Description = "Churchgate to Malad Western Line",
            StartStationId = Guid.Parse("44444444-4444-4444-4444-444444444444"), // Churchgate
            EndStationId = Guid.Parse("99999999-9999-9999-9999-999999999999"), // Malad
            TransportModeId = Guid.Parse("33333333-3333-3333-3333-333333333333"), // Train
            TotalDistance = 32.50m,
            BaseFare = 15.00m,
            IsActive = true,
            CreatedAt = seedDate,
        },

        // Metro Route: Blue Line
        new Core.Models.Route
        {
            Id = Guid.Parse("40404040-4040-4040-4040-404040404040"),
            Name = "Blue Line",
            Code = "BL01",
            Description = "Versova to Ghatkopar Metro Line",
            StartStationId = Guid.Parse("cccccccc-cccc-cccc-cccc-cccccccccccc"), // Versova
            EndStationId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"), // Ghatkopar
            TransportModeId = Guid.Parse("22222222-2222-2222-2222-222222222222"), // Metro
            TotalDistance = 28.30m,
            BaseFare = 20.00m,
            IsActive = true,
            CreatedAt = seedDate,
        },

        // Bus Routes
        new Core.Models.Route
        {
            Id = Guid.Parse("50505050-5050-5050-5050-505050505050"),
            Name = "Bandra Circular",
            Code = "B201",
            Description = "Bandra Terminal to BKC via Juhu",
            StartStationId = Guid.Parse("eeeeeeee-eeee-eeee-eeee-eeeeeeeeeeee"), // Bandra Terminal
            EndStationId = Guid.Parse("20202020-2020-2020-2020-202020202020"), // BKC
            TransportModeId = Guid.Parse("11111111-1111-1111-1111-111111111111"), // Bus
            TotalDistance = 15.20m,
            BaseFare = 8.00m,
            IsActive = true,
            CreatedAt = seedDate,
        },

        new Core.Models.Route
        {
            Id = Guid.Parse("60606060-6060-6060-6060-606060606060"),
            Name = "Eastern Express",
            Code = "B301",
            Description = "Powai to Ghatkopar Express",
            StartStationId = Guid.Parse("10101010-1010-1010-1010-101010101010"), // Powai
            EndStationId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"), // Ghatkopar
            TransportModeId = Guid.Parse("11111111-1111-1111-1111-111111111111"), // Bus
            TotalDistance = 8.50m,
            BaseFare = 6.00m,
            IsActive = true,
            CreatedAt = seedDate,
        }
    };

            modelBuilder.Entity<Core.Models.Route>().HasData(routes);

            // Seed RouteStations
            var routeStations = new[]
            {
        // Western Line Route Stations
        new RouteStation
        {
            Id = Guid.Parse("70707070-7070-7070-7070-707070707070"),
            RouteId = Guid.Parse("30303030-3030-3030-3030-303030303030"),
            StationId = Guid.Parse("44444444-4444-4444-4444-444444444444"), // Churchgate
            StationOrder = 1,
            DistanceFromStart = 0.00m,
            FareFromStart = 0.00m,
            CreatedAt = seedDate,
        },
        new RouteStation
        {
            Id = Guid.Parse("71717171-7171-7171-7171-717171717171"),
            RouteId = Guid.Parse("30303030-3030-3030-3030-303030303030"),
            StationId = Guid.Parse("55555555-5555-5555-5555-555555555555"), // Marine Lines
            StationOrder = 2,
            DistanceFromStart = 2.50m,
            FareFromStart = 3.75m,
            CreatedAt = seedDate,
        },
        new RouteStation
        {
            Id = Guid.Parse("72727272-7272-7272-7272-727272727272"),
            RouteId = Guid.Parse("30303030-3030-3030-3030-303030303030"),
            StationId = Guid.Parse("66666666-6666-6666-6666-666666666666"), // Charni Road
            StationOrder = 3,
            DistanceFromStart = 4.80m,
            FareFromStart = 7.20m,
            CreatedAt = seedDate,
        },
        new RouteStation
        {
            Id = Guid.Parse("73737373-7373-7373-7373-737373737373"),
            RouteId = Guid.Parse("30303030-3030-3030-3030-303030303030"),
            StationId = Guid.Parse("77777777-7777-7777-7777-777777777777"), // Grant Road
            StationOrder = 4,
            DistanceFromStart = 7.20m,
            FareFromStart = 10.80m,
            CreatedAt = seedDate,
        },
        new RouteStation
        {
            Id = Guid.Parse("74747474-7474-7474-7474-747474747474"),
            RouteId = Guid.Parse("30303030-3030-3030-3030-303030303030"),
            StationId = Guid.Parse("88888888-8888-8888-8888-888888888888"), // Mumbai Central
            StationOrder = 5,
            DistanceFromStart = 9.50m,
            FareFromStart = 14.25m,
            CreatedAt = seedDate,
        },
        new RouteStation
        {
            Id = Guid.Parse("75757575-7575-7575-7575-757575757575"),
            RouteId = Guid.Parse("30303030-3030-3030-3030-303030303030"),
            StationId = Guid.Parse("99999999-9999-9999-9999-999999999999"), // Malad
            StationOrder = 6,
            DistanceFromStart = 32.50m,
            FareFromStart = 48.75m,
            CreatedAt = seedDate,
        },

        // Metro Blue Line Route Stations
        new RouteStation
        {
            Id = Guid.Parse("80808080-8080-8080-8080-808080808080"),
            RouteId = Guid.Parse("40404040-4040-4040-4040-404040404040"),
            StationId = Guid.Parse("cccccccc-cccc-cccc-cccc-cccccccccccc"), // Versova
            StationOrder = 1,
            DistanceFromStart = 0.00m,
            FareFromStart = 0.00m,
            CreatedAt = seedDate,
        },
        new RouteStation
        {
            Id = Guid.Parse("81818181-8181-8181-8181-818181818181"),
            RouteId = Guid.Parse("40404040-4040-4040-4040-404040404040"),
            StationId = Guid.Parse("dddddddd-dddd-dddd-dddd-dddddddddddd"), // Azad Nagar
            StationOrder = 2,
            DistanceFromStart = 8.50m,
            FareFromStart = 25.50m,
            CreatedAt = seedDate,
        },
        new RouteStation
        {
            Id = Guid.Parse("82828282-8282-8282-8282-828282828282"),
            RouteId = Guid.Parse("40404040-4040-4040-4040-404040404040"),
            StationId = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"), // Andheri
            StationOrder = 3,
            DistanceFromStart = 12.80m,
            FareFromStart = 38.40m,
            CreatedAt = seedDate,
        },
        new RouteStation
        {
            Id = Guid.Parse("83838383-8383-8383-8383-838383838383"),
            RouteId = Guid.Parse("40404040-4040-4040-4040-404040404040"),
            StationId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"), // Ghatkopar
            StationOrder = 4,
            DistanceFromStart = 28.30m,
            FareFromStart = 84.90m,
            CreatedAt = seedDate,
        },

        // Bus Route 1: Bandra Circular
        new RouteStation
        {
            Id = Guid.Parse("90909090-9090-9090-9090-909090909090"),
            RouteId = Guid.Parse("50505050-5050-5050-5050-505050505050"),
            StationId = Guid.Parse("eeeeeeee-eeee-eeee-eeee-eeeeeeeeeeee"), // Bandra Terminal
            StationOrder = 1,
            DistanceFromStart = 0.00m,
            FareFromStart = 0.00m,
            CreatedAt = seedDate,
        },
        new RouteStation
        {
            Id = Guid.Parse("91919191-9191-9191-9191-919191919191"),
            RouteId = Guid.Parse("50505050-5050-5050-5050-505050505050"),
            StationId = Guid.Parse("ffffffff-ffff-ffff-ffff-ffffffffffff"), // Juhu Beach
            StationOrder = 2,
            DistanceFromStart = 8.20m,
            FareFromStart = 16.40m,
            CreatedAt = seedDate,
        },
        new RouteStation
        {
            Id = Guid.Parse("92929292-9292-9292-9292-929292929292"),
            RouteId = Guid.Parse("50505050-5050-5050-5050-505050505050"),
            StationId = Guid.Parse("20202020-2020-2020-2020-202020202020"), // BKC
            StationOrder = 3,
            DistanceFromStart = 15.20m,
            FareFromStart = 30.40m,
            CreatedAt = seedDate,
        },

        // Bus Route 2: Eastern Express
        new RouteStation
        {
            Id = Guid.Parse("a0a0a0a0-a0a0-a0a0-a0a0-a0a0a0a0a0a0"),
            RouteId = Guid.Parse("60606060-6060-6060-6060-606060606060"),
            StationId = Guid.Parse("10101010-1010-1010-1010-101010101010"), // Powai
            StationOrder = 1,
            DistanceFromStart = 0.00m,
            FareFromStart = 0.00m,
            CreatedAt = seedDate,
        },
        new RouteStation
        {
            Id = Guid.Parse("a1a1a1a1-a1a1-a1a1-a1a1-a1a1a1a1a1a1"),
            RouteId = Guid.Parse("60606060-6060-6060-6060-606060606060"),
            StationId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"), // Ghatkopar
            StationOrder = 2,
            DistanceFromStart = 8.50m,
            FareFromStart = 17.00m,
            CreatedAt = seedDate,
        }
    };

            modelBuilder.Entity<RouteStation>().HasData(routeStations);

            // Seed Schedules
            var schedules = new List<Schedule>();
            var scheduleId = 1;

            // Western Line Train Schedules (6 AM to 11 PM, every 15 minutes)
            var westernLineStations = new[]
            {
        Guid.Parse("44444444-4444-4444-4444-444444444444"), // Churchgate
        Guid.Parse("55555555-5555-5555-5555-555555555555"), // Marine Lines
        Guid.Parse("66666666-6666-6666-6666-666666666666"), // Charni Road
        Guid.Parse("77777777-7777-7777-7777-777777777777"), // Grant Road
        Guid.Parse("88888888-8888-8888-8888-888888888888"), // Mumbai Central
        Guid.Parse("99999999-9999-9999-9999-999999999999")  // Malad
    };

            var travelTimes = new[] { 0, 5, 8, 12, 15, 35 }; // Minutes from Churchgate

            for (int hour = 6; hour <= 23; hour++)
            {
                for (int minute = 0; minute < 60; minute += 15)
                {
                    var baseTime = new TimeSpan(hour, minute, 0);

                    for (int i = 0; i < westernLineStations.Length; i++)
                    {
                        schedules.Add(new Schedule
                        {
                            Id = Guid.Parse($"b{scheduleId:D7}-bbbb-bbbb-bbbb-bbbbbbbbbbbb"),
                            RouteId = Guid.Parse("30303030-3030-3030-3030-303030303030"),
                            StationId = westernLineStations[i],
                            DepartureTime = baseTime.Add(TimeSpan.FromMinutes(travelTimes[i])),
                            ArrivalTime = i == 0 ? baseTime : baseTime.Add(TimeSpan.FromMinutes(travelTimes[i] - 1)),
                            IsActive = true,
                            CreatedAt = seedDate,
                        });
                        scheduleId++;
                    }
                }
            }

            // Metro Blue Line Schedules (5 AM to 12 AM, every 8 minutes)
            var metroStations = new[]
            {
        Guid.Parse("cccccccc-cccc-cccc-cccc-cccccccccccc"), // Versova
        Guid.Parse("dddddddd-dddd-dddd-dddd-dddddddddddd"), // Azad Nagar
        Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"), // Andheri
        Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa")  // Ghatkopar
    };

            var metroTravelTimes = new[] { 0, 12, 18, 35 }; // Minutes from Versova

            for (int hour = 5; hour <= 23; hour++)
            {
                for (int minute = 0; minute < 60; minute += 8)
                {
                    var baseTime = new TimeSpan(hour, minute, 0);

                    for (int i = 0; i < metroStations.Length; i++)
                    {
                        schedules.Add(new Schedule
                        {
                            Id = Guid.Parse($"c{scheduleId:D7}-cccc-cccc-cccc-cccccccccccc"),
                            RouteId = Guid.Parse("40404040-4040-4040-4040-404040404040"),
                            StationId = metroStations[i],
                            DepartureTime = baseTime.Add(TimeSpan.FromMinutes(metroTravelTimes[i])),
                            ArrivalTime = i == 0 ? baseTime : baseTime.Add(TimeSpan.FromMinutes(metroTravelTimes[i] - 1)),
                            IsActive = true,
                            CreatedAt = seedDate,
                        });
                        scheduleId++;
                    }
                }
            }

            // Bus Schedules (6 AM to 10 PM, every 30 minutes)
            var busRoutes = new[]
            {
        new { RouteId = Guid.Parse("50505050-5050-5050-5050-505050505050"), Stations = new[] {
            Guid.Parse("eeeeeeee-eeee-eeee-eeee-eeeeeeeeeeee"), // Bandra Terminal
            Guid.Parse("ffffffff-ffff-ffff-ffff-ffffffffffff"), // Juhu Beach
            Guid.Parse("20202020-2020-2020-2020-202020202020")  // BKC
        }, TravelTimes = new[] { 0, 15, 25 } },
        new { RouteId = Guid.Parse("60606060-6060-6060-6060-606060606060"), Stations = new[] {
            Guid.Parse("10101010-1010-1010-1010-101010101010"), // Powai
            Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa")  // Ghatkopar
        }, TravelTimes = new[] { 0, 20 } }
    };

            foreach (var busRoute in busRoutes)
            {
                for (int hour = 6; hour <= 22; hour++)
                {
                    for (int minute = 0; minute < 60; minute += 30)
                    {
                        var baseTime = new TimeSpan(hour, minute, 0);

                        for (int i = 0; i < busRoute.Stations.Length; i++)
                        {
                            schedules.Add(new Schedule
                            {
                                Id = Guid.Parse($"d{scheduleId:D7}-dddd-dddd-dddd-dddddddddddd"),
                                RouteId = busRoute.RouteId,
                                StationId = busRoute.Stations[i],
                                DepartureTime = baseTime.Add(TimeSpan.FromMinutes(busRoute.TravelTimes[i])),
                                ArrivalTime = i == 0 ? baseTime : baseTime.Add(TimeSpan.FromMinutes(busRoute.TravelTimes[i] - 2)),
                                IsActive = true,
                                CreatedAt = seedDate,
                            });
                            scheduleId++;
                        }
                    }
                }
            }

            modelBuilder.Entity<Schedule>().HasData(schedules.ToArray());
        }
    }
}