using System;
using Microsoft.EntityFrameworkCore.Migrations;
using NetTopologySuite.Geometries;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace TransitAFC.Services.Route.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialRouteCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:PostgresExtension:postgis", ",,");

            migrationBuilder.CreateTable(
                name: "TransportModes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Code = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    IconUrl = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Color = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    BaseFare = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: false),
                    FarePerKm = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: false),
                    MaxCapacity = table.Column<int>(type: "integer", nullable: false),
                    IsRealTimeEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TransportModes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Stations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Code = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Address = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    City = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    State = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    PinCode = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    Location = table.Column<Point>(type: "geography (point)", nullable: false),
                    Latitude = table.Column<double>(type: "double precision", nullable: false),
                    Longitude = table.Column<double>(type: "double precision", nullable: false),
                    TransportModeId = table.Column<Guid>(type: "uuid", nullable: false),
                    StationType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    HasWheelchairAccess = table.Column<bool>(type: "boolean", nullable: false),
                    HasParking = table.Column<bool>(type: "boolean", nullable: false),
                    HasWiFi = table.Column<bool>(type: "boolean", nullable: false),
                    HasRestroom = table.Column<bool>(type: "boolean", nullable: false),
                    Amenities = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    PlatformCount = table.Column<int>(type: "integer", nullable: false),
                    LastUpdated = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Stations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Stations_TransportModes_TransportModeId",
                        column: x => x.TransportModeId,
                        principalTable: "TransportModes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Routes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Code = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    TransportModeId = table.Column<Guid>(type: "uuid", nullable: false),
                    StartStationId = table.Column<Guid>(type: "uuid", nullable: false),
                    EndStationId = table.Column<Guid>(type: "uuid", nullable: false),
                    TotalDistance = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: false),
                    EstimatedDuration = table.Column<TimeSpan>(type: "interval", nullable: false),
                    BaseFare = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: false),
                    RouteColor = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    IsExpress = table.Column<bool>(type: "boolean", nullable: false),
                    ServiceStartTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ServiceEndTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    FrequencyMinutes = table.Column<int>(type: "integer", nullable: false),
                    MaxCapacity = table.Column<int>(type: "integer", nullable: true),
                    RouteGeometry = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Routes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Routes_Stations_EndStationId",
                        column: x => x.EndStationId,
                        principalTable: "Stations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Routes_Stations_StartStationId",
                        column: x => x.StartStationId,
                        principalTable: "Stations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Routes_TransportModes_TransportModeId",
                        column: x => x.TransportModeId,
                        principalTable: "TransportModes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "RouteStations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    RouteId = table.Column<Guid>(type: "uuid", nullable: false),
                    StationId = table.Column<Guid>(type: "uuid", nullable: false),
                    StationOrder = table.Column<int>(type: "integer", nullable: false),
                    DistanceFromStart = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: false),
                    EstimatedTravelTime = table.Column<TimeSpan>(type: "interval", nullable: false),
                    FareFromStart = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: false),
                    IsStopRequired = table.Column<bool>(type: "boolean", nullable: false),
                    DwellTime = table.Column<TimeSpan>(type: "interval", nullable: true),
                    Notes = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RouteStations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RouteStations_Routes_RouteId",
                        column: x => x.RouteId,
                        principalTable: "Routes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RouteStations_Stations_StationId",
                        column: x => x.StationId,
                        principalTable: "Stations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Schedules",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    RouteId = table.Column<Guid>(type: "uuid", nullable: false),
                    StationId = table.Column<Guid>(type: "uuid", nullable: false),
                    DepartureTime = table.Column<TimeSpan>(type: "interval", nullable: false),
                    ArrivalTime = table.Column<TimeSpan>(type: "interval", nullable: true),
                    DayOfWeek = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    IsWeekday = table.Column<bool>(type: "boolean", nullable: false),
                    IsWeekend = table.Column<bool>(type: "boolean", nullable: false),
                    IsHoliday = table.Column<bool>(type: "boolean", nullable: false),
                    VehicleNumber = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    VehicleCapacity = table.Column<int>(type: "integer", nullable: true),
                    EffectiveFrom = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EffectiveTo = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Notes = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Schedules", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Schedules_Routes_RouteId",
                        column: x => x.RouteId,
                        principalTable: "Routes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Schedules_Stations_StationId",
                        column: x => x.StationId,
                        principalTable: "Stations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "TransportModes",
                columns: new[] { "Id", "BaseFare", "Code", "Color", "CreatedAt", "Description", "FarePerKm", "IconUrl", "IsActive", "IsDeleted", "IsRealTimeEnabled", "MaxCapacity", "Name", "UpdatedAt" },
                values: new object[,]
                {
                    { new Guid("11111111-1111-1111-1111-111111111111"), 5.00m, "BUS", "#FF6B35", new DateTime(2025, 9, 15, 13, 36, 37, 489, DateTimeKind.Utc).AddTicks(1519), "Public bus transportation", 2.00m, null, true, false, true, 50, "Bus", null },
                    { new Guid("22222222-2222-2222-2222-222222222222"), 10.00m, "MET", "#004E89", new DateTime(2025, 9, 15, 13, 36, 37, 489, DateTimeKind.Utc).AddTicks(2572), "Metro rail system", 3.00m, null, true, false, true, 200, "Metro", null },
                    { new Guid("33333333-3333-3333-3333-333333333333"), 15.00m, "TRN", "#009639", new DateTime(2025, 9, 15, 13, 36, 37, 489, DateTimeKind.Utc).AddTicks(2578), "Railway transportation", 1.50m, null, true, false, false, 500, "Train", null }
                });

            migrationBuilder.InsertData(
                table: "Stations",
                columns: new[] { "Id", "Address", "Amenities", "City", "Code", "CreatedAt", "HasParking", "HasRestroom", "HasWheelchairAccess", "HasWiFi", "IsActive", "IsDeleted", "LastUpdated", "Latitude", "Location", "Longitude", "Name", "PinCode", "PlatformCount", "State", "StationType", "TransportModeId", "UpdatedAt" },
                values: new object[,]
                {
                    { new Guid("44444444-4444-4444-4444-444444444444"), "Veer Nariman Road, Fort", "[\"WiFi\",\"Restroom\",\"ATM\",\"Food Court\"]", "Mumbai", "CG", new DateTime(2025, 9, 15, 13, 36, 37, 500, DateTimeKind.Utc).AddTicks(534), true, true, true, true, true, false, null, 18.932200000000002, (NetTopologySuite.Geometries.Point)new NetTopologySuite.IO.WKTReader().Read("POINT (72.8264 18.9322)"), 72.826400000000007, "Churchgate", "400001", 6, "Maharashtra", "Terminal", new Guid("33333333-3333-3333-3333-333333333333"), null },
                    { new Guid("55555555-5555-5555-5555-555555555555"), "Marine Drive", "[\"WiFi\",\"Restroom\",\"ATM\",\"Food Court\"]", "Mumbai", "ML", new DateTime(2025, 9, 15, 13, 36, 37, 500, DateTimeKind.Utc).AddTicks(2295), false, true, true, true, true, false, null, 18.9467, (NetTopologySuite.Geometries.Point)new NetTopologySuite.IO.WKTReader().Read("POINT (72.8233 18.9467)"), 72.823300000000003, "Marine Lines", "400002", 4, "Maharashtra", "Regular", new Guid("33333333-3333-3333-3333-333333333333"), null }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Routes_Code",
                table: "Routes",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Routes_EndStationId",
                table: "Routes",
                column: "EndStationId");

            migrationBuilder.CreateIndex(
                name: "IX_Routes_StartStationId_EndStationId",
                table: "Routes",
                columns: new[] { "StartStationId", "EndStationId" });

            migrationBuilder.CreateIndex(
                name: "IX_Routes_TransportModeId",
                table: "Routes",
                column: "TransportModeId");

            migrationBuilder.CreateIndex(
                name: "IX_RouteStations_RouteId_StationId",
                table: "RouteStations",
                columns: new[] { "RouteId", "StationId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RouteStations_RouteId_StationOrder",
                table: "RouteStations",
                columns: new[] { "RouteId", "StationOrder" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RouteStations_StationId",
                table: "RouteStations",
                column: "StationId");

            migrationBuilder.CreateIndex(
                name: "IX_Schedules_RouteId_StationId_DepartureTime",
                table: "Schedules",
                columns: new[] { "RouteId", "StationId", "DepartureTime" });

            migrationBuilder.CreateIndex(
                name: "IX_Schedules_StationId",
                table: "Schedules",
                column: "StationId");

            migrationBuilder.CreateIndex(
                name: "IX_Stations_Code",
                table: "Stations",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Stations_Location",
                table: "Stations",
                column: "Location")
                .Annotation("Npgsql:IndexMethod", "GIST");

            migrationBuilder.CreateIndex(
                name: "IX_Stations_TransportModeId",
                table: "Stations",
                column: "TransportModeId");

            migrationBuilder.CreateIndex(
                name: "IX_TransportModes_Code",
                table: "TransportModes",
                column: "Code",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RouteStations");

            migrationBuilder.DropTable(
                name: "Schedules");

            migrationBuilder.DropTable(
                name: "Routes");

            migrationBuilder.DropTable(
                name: "Stations");

            migrationBuilder.DropTable(
                name: "TransportModes");
        }
    }
}
