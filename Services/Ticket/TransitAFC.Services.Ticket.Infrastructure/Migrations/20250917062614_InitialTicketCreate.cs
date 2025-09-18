using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TransitAFC.Services.Ticket.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialTicketCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Tickets",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TicketNumber = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    BookingId = table.Column<Guid>(type: "uuid", nullable: false),
                    PaymentId = table.Column<Guid>(type: "uuid", nullable: false),
                    BookingNumber = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Status = table.Column<string>(type: "text", nullable: false),
                    Type = table.Column<string>(type: "text", nullable: false),
                    TransportMode = table.Column<string>(type: "text", nullable: false),
                    FareType = table.Column<string>(type: "text", nullable: false),
                    SourceStationId = table.Column<Guid>(type: "uuid", nullable: false),
                    SourceStationName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    SourceStationCode = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    DestinationStationId = table.Column<Guid>(type: "uuid", nullable: false),
                    DestinationStationName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    DestinationStationCode = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    RouteId = table.Column<Guid>(type: "uuid", nullable: false),
                    RouteName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    RouteCode = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    BasePrice = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: false),
                    DiscountAmount = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: true),
                    TaxAmount = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: true),
                    FinalPrice = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: false),
                    Currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false, defaultValue: "INR"),
                    ValidFrom = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ValidUntil = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    FirstUsedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastUsedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    MaxUsageCount = table.Column<int>(type: "integer", nullable: false),
                    UsageCount = table.Column<int>(type: "integer", nullable: false),
                    EstimatedDuration = table.Column<decimal>(type: "numeric(8,2)", precision: 8, scale: 2, nullable: false),
                    EstimatedDistance = table.Column<decimal>(type: "numeric(8,2)", precision: 8, scale: 2, nullable: false),
                    PassengerName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    PassengerAge = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    PassengerType = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false, defaultValue: "Adult"),
                    PassengerPhone = table.Column<string>(type: "character varying(15)", maxLength: 15, nullable: false),
                    PassengerEmail = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    QRCodeData = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    QRCodeHash = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    QRCodeImage = table.Column<byte[]>(type: "bytea", nullable: true),
                    ValidatedByStationId = table.Column<Guid>(type: "uuid", nullable: true),
                    ValidatedByStationName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    ValidatedByDeviceId = table.Column<Guid>(type: "uuid", nullable: true),
                    ValidatedByDeviceName = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    ValidatedByOperatorId = table.Column<Guid>(type: "uuid", nullable: true),
                    ValidatedByOperatorName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    ZoneFrom = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    ZoneTo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    ServiceClass = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true, defaultValue: "Standard"),
                    SeatNumber = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    CoachNumber = table.Column<string>(type: "character varying(5)", maxLength: 5, nullable: true),
                    SpecialInstructions = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    CancellationReason = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Notes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    AllowsTransfer = table.Column<bool>(type: "boolean", nullable: false),
                    MaxTransfers = table.Column<int>(type: "integer", nullable: false),
                    TransferCount = table.Column<int>(type: "integer", nullable: false),
                    TransferTimeLimit = table.Column<decimal>(type: "numeric(8,2)", precision: 8, scale: 2, nullable: true),
                    Metadata = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    RegulatoryCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    OperatorCode = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    IsRefundable = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    IsTransferable = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    RequiresValidation = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tickets", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TicketHistory",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TicketId = table.Column<Guid>(type: "uuid", nullable: false),
                    FromStatus = table.Column<string>(type: "text", nullable: false),
                    ToStatus = table.Column<string>(type: "text", nullable: false),
                    Action = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Reason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Notes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    ActionBy = table.Column<Guid>(type: "uuid", nullable: false),
                    ActionByType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    ActionByName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    IpAddress = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    UserAgent = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    ActionTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    StationId = table.Column<Guid>(type: "uuid", nullable: true),
                    StationName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    DeviceId = table.Column<Guid>(type: "uuid", nullable: true),
                    DeviceName = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    ChangeDetails = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    ReferenceId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    AdditionalData = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TicketHistory", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TicketHistory_Tickets_TicketId",
                        column: x => x.TicketId,
                        principalTable: "Tickets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TicketQRCodes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TicketId = table.Column<Guid>(type: "uuid", nullable: false),
                    QRCodeId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    QRCodeData = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    QRCodeHash = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    QRCodeImage = table.Column<byte[]>(type: "bytea", nullable: false),
                    Status = table.Column<string>(type: "text", nullable: false),
                    GeneratedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UsedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    InvalidatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Size = table.Column<int>(type: "integer", nullable: false),
                    Version = table.Column<int>(type: "integer", nullable: false),
                    Format = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false, defaultValue: "PNG"),
                    ErrorCorrectionLevel = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false, defaultValue: "M"),
                    EncryptionKey = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    EncryptionAlgorithm = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false, defaultValue: "AES256"),
                    Salt = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    ScanCount = table.Column<int>(type: "integer", nullable: false),
                    LastScannedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastScannedBy = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    LastScannedDevice = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    IsRegenerated = table.Column<bool>(type: "boolean", nullable: false),
                    PreviousQRCodeId = table.Column<Guid>(type: "uuid", nullable: true),
                    NextQRCodeId = table.Column<Guid>(type: "uuid", nullable: true),
                    RegenerationReason = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    DisplayText = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Instructions = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    AdditionalData = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TicketQRCodes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TicketQRCodes_Tickets_TicketId",
                        column: x => x.TicketId,
                        principalTable: "Tickets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TicketTransfers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TicketId = table.Column<Guid>(type: "uuid", nullable: false),
                    TransferId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    TransferSequence = table.Column<int>(type: "integer", nullable: false),
                    FromStationId = table.Column<Guid>(type: "uuid", nullable: false),
                    FromStationName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    FromStationCode = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    FromTransportMode = table.Column<string>(type: "text", nullable: false),
                    FromRouteNumber = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    FromVehicleNumber = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    ExitTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ToStationId = table.Column<Guid>(type: "uuid", nullable: false),
                    ToStationName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    ToStationCode = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    ToTransportMode = table.Column<string>(type: "text", nullable: false),
                    ToRouteNumber = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    ToVehicleNumber = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    EntryTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    TransferTime = table.Column<decimal>(type: "numeric(8,2)", precision: 8, scale: 2, nullable: false),
                    WalkingDistance = table.Column<decimal>(type: "numeric(8,2)", precision: 8, scale: 2, nullable: false),
                    IsValidTransfer = table.Column<bool>(type: "boolean", nullable: false),
                    IsWithinTimeLimit = table.Column<bool>(type: "boolean", nullable: false),
                    IsWithinDistance = table.Column<bool>(type: "boolean", nullable: false),
                    TransferFee = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: false),
                    AdditionalFare = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: false),
                    TransferReason = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    TransferNotes = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    ExitValidationId = table.Column<Guid>(type: "uuid", nullable: true),
                    EntryValidationId = table.Column<Guid>(type: "uuid", nullable: true),
                    TransferCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    AuthorizedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    AuthorizedByName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    AuthorizedByBadge = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TicketTransfers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TicketTransfers_Tickets_TicketId",
                        column: x => x.TicketId,
                        principalTable: "Tickets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TicketValidations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TicketId = table.Column<Guid>(type: "uuid", nullable: false),
                    ValidationId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ValidationType = table.Column<string>(type: "text", nullable: false),
                    ValidationResult = table.Column<string>(type: "text", nullable: false),
                    ValidationTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    StationId = table.Column<Guid>(type: "uuid", nullable: false),
                    StationName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    StationCode = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    ZoneCode = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    DeviceId = table.Column<Guid>(type: "uuid", nullable: false),
                    DeviceName = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    DeviceType = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    DeviceSerialNumber = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    OperatorId = table.Column<Guid>(type: "uuid", nullable: true),
                    OperatorName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    OperatorBadgeNumber = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    ValidationMethod = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    ValidationData = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    ValidationError = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Latitude = table.Column<decimal>(type: "numeric(10,7)", precision: 10, scale: 7, nullable: true),
                    Longitude = table.Column<decimal>(type: "numeric(10,7)", precision: 10, scale: 7, nullable: true),
                    LocationDescription = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Direction = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    Platform = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Gate = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    FareDeducted = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: true),
                    BalanceAfter = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: true),
                    TripId = table.Column<Guid>(type: "uuid", nullable: true),
                    VehicleNumber = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    RouteNumber = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    SystemVersion = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    ProcessingTime = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    TransactionId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    RequiresManualReview = table.Column<bool>(type: "boolean", nullable: false),
                    ReviewNotes = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    ReviewedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ReviewedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TicketValidations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TicketValidations_Tickets_TicketId",
                        column: x => x.TicketId,
                        principalTable: "Tickets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TicketHistory_ActionBy",
                table: "TicketHistory",
                column: "ActionBy");

            migrationBuilder.CreateIndex(
                name: "IX_TicketHistory_ActionTime",
                table: "TicketHistory",
                column: "ActionTime");

            migrationBuilder.CreateIndex(
                name: "IX_TicketHistory_TicketId",
                table: "TicketHistory",
                column: "TicketId");

            migrationBuilder.CreateIndex(
                name: "IX_TicketHistory_TicketId_ActionTime",
                table: "TicketHistory",
                columns: new[] { "TicketId", "ActionTime" });

            migrationBuilder.CreateIndex(
                name: "IX_TicketQRCodes_ExpiresAt",
                table: "TicketQRCodes",
                column: "ExpiresAt");

            migrationBuilder.CreateIndex(
                name: "IX_TicketQRCodes_QRCodeHash",
                table: "TicketQRCodes",
                column: "QRCodeHash");

            migrationBuilder.CreateIndex(
                name: "IX_TicketQRCodes_QRCodeId",
                table: "TicketQRCodes",
                column: "QRCodeId");

            migrationBuilder.CreateIndex(
                name: "IX_TicketQRCodes_Status",
                table: "TicketQRCodes",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_TicketQRCodes_TicketId",
                table: "TicketQRCodes",
                column: "TicketId");

            migrationBuilder.CreateIndex(
                name: "IX_TicketQRCodes_TicketId_Status",
                table: "TicketQRCodes",
                columns: new[] { "TicketId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_Tickets_BookingId",
                table: "Tickets",
                column: "BookingId");

            migrationBuilder.CreateIndex(
                name: "IX_Tickets_CreatedAt_Status",
                table: "Tickets",
                columns: new[] { "CreatedAt", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_Tickets_PaymentId",
                table: "Tickets",
                column: "PaymentId");

            migrationBuilder.CreateIndex(
                name: "IX_Tickets_QRCodeHash",
                table: "Tickets",
                column: "QRCodeHash");

            migrationBuilder.CreateIndex(
                name: "IX_Tickets_SourceStationId_DestinationStationId",
                table: "Tickets",
                columns: new[] { "SourceStationId", "DestinationStationId" });

            migrationBuilder.CreateIndex(
                name: "IX_Tickets_Status",
                table: "Tickets",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_Tickets_TicketNumber",
                table: "Tickets",
                column: "TicketNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Tickets_TransportMode",
                table: "Tickets",
                column: "TransportMode");

            migrationBuilder.CreateIndex(
                name: "IX_Tickets_Type",
                table: "Tickets",
                column: "Type");

            migrationBuilder.CreateIndex(
                name: "IX_Tickets_UserId",
                table: "Tickets",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Tickets_UserId_Status",
                table: "Tickets",
                columns: new[] { "UserId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_Tickets_ValidFrom_ValidUntil",
                table: "Tickets",
                columns: new[] { "ValidFrom", "ValidUntil" });

            migrationBuilder.CreateIndex(
                name: "IX_TicketTransfers_EntryTime",
                table: "TicketTransfers",
                column: "EntryTime");

            migrationBuilder.CreateIndex(
                name: "IX_TicketTransfers_ExitTime",
                table: "TicketTransfers",
                column: "ExitTime");

            migrationBuilder.CreateIndex(
                name: "IX_TicketTransfers_FromStationId",
                table: "TicketTransfers",
                column: "FromStationId");

            migrationBuilder.CreateIndex(
                name: "IX_TicketTransfers_TicketId",
                table: "TicketTransfers",
                column: "TicketId");

            migrationBuilder.CreateIndex(
                name: "IX_TicketTransfers_TicketId_TransferSequence",
                table: "TicketTransfers",
                columns: new[] { "TicketId", "TransferSequence" });

            migrationBuilder.CreateIndex(
                name: "IX_TicketTransfers_ToStationId",
                table: "TicketTransfers",
                column: "ToStationId");

            migrationBuilder.CreateIndex(
                name: "IX_TicketTransfers_TransferId",
                table: "TicketTransfers",
                column: "TransferId");

            migrationBuilder.CreateIndex(
                name: "IX_TicketValidations_DeviceId",
                table: "TicketValidations",
                column: "DeviceId");

            migrationBuilder.CreateIndex(
                name: "IX_TicketValidations_StationId",
                table: "TicketValidations",
                column: "StationId");

            migrationBuilder.CreateIndex(
                name: "IX_TicketValidations_StationId_ValidationTime",
                table: "TicketValidations",
                columns: new[] { "StationId", "ValidationTime" });

            migrationBuilder.CreateIndex(
                name: "IX_TicketValidations_TicketId",
                table: "TicketValidations",
                column: "TicketId");

            migrationBuilder.CreateIndex(
                name: "IX_TicketValidations_TicketId_ValidationTime",
                table: "TicketValidations",
                columns: new[] { "TicketId", "ValidationTime" });

            migrationBuilder.CreateIndex(
                name: "IX_TicketValidations_ValidationId",
                table: "TicketValidations",
                column: "ValidationId");

            migrationBuilder.CreateIndex(
                name: "IX_TicketValidations_ValidationResult",
                table: "TicketValidations",
                column: "ValidationResult");

            migrationBuilder.CreateIndex(
                name: "IX_TicketValidations_ValidationTime",
                table: "TicketValidations",
                column: "ValidationTime");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TicketHistory");

            migrationBuilder.DropTable(
                name: "TicketQRCodes");

            migrationBuilder.DropTable(
                name: "TicketTransfers");

            migrationBuilder.DropTable(
                name: "TicketValidations");

            migrationBuilder.DropTable(
                name: "Tickets");
        }
    }
}
