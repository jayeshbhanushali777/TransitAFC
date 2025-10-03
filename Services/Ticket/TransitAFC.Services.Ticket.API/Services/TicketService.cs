using AutoMapper;
using Microsoft.Extensions.Logging;
using TransitAFC.Services.Ticket.API.Services;
using TransitAFC.Services.Ticket.Core.DTOs;
using TransitAFC.Services.Ticket.Core.Models;
using TransitAFC.Services.Ticket.Infrastructure.Repositories;
using TransitAFC.Services.Ticket.Infrastructure.Services;

namespace TransitAFC.Services.Ticket.API.Services
{
    public class TicketService : ITicketService
    {
        private readonly ITicketRepository _ticketRepository;
        private readonly ITicketValidationRepository _validationRepository;
        private readonly ITicketQRCodeRepository _qrCodeRepository;
        private readonly ITicketHistoryRepository _historyRepository;
        private readonly IQRCodeService _qrCodeService;
        private readonly IPaymentService _paymentService;
        private readonly IBookingService _bookingService;
        private readonly INotificationService _notificationService;
        private readonly IMapper _mapper;
        private readonly ILogger<TicketService> _logger;

        public TicketService(
            ITicketRepository ticketRepository,
            ITicketValidationRepository validationRepository,
            ITicketQRCodeRepository qrCodeRepository,
            ITicketHistoryRepository historyRepository,
            IQRCodeService qrCodeService,
            IPaymentService paymentService,
            IBookingService bookingService,
            INotificationService notificationService,
            IMapper mapper,
            ILogger<TicketService> logger)
        {
            _ticketRepository = ticketRepository;
            _validationRepository = validationRepository;
            _qrCodeRepository = qrCodeRepository;
            _historyRepository = historyRepository;
            _qrCodeService = qrCodeService;
            _paymentService = paymentService;
            _bookingService = bookingService;
            _notificationService = notificationService;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<TicketResponse> CreateTicketAsync(Guid userId, CreateTicketRequest request)
        {
            try
            {
                _logger.LogInformation("Creating ticket for user {UserId}, booking {BookingId}", userId, request.BookingId);

                // Validate booking exists and is confirmed
                var booking = await _bookingService.GetBookingAsync(request.BookingId, userId);
                if (booking == null)
                {
                    throw new InvalidOperationException("Booking not found or access denied");
                }

                if (booking.Status != "Confirmed")
                {
                    throw new InvalidOperationException("Booking must be confirmed before generating ticket");
                }

                // Validate payment is completed
                var payment = await _paymentService.GetPaymentByBookingIdAsync(request.BookingId, userId);
                if (payment == null || payment.Status != "Completed")
                {
                    throw new InvalidOperationException("Payment must be completed before generating ticket");
                }

                // Check if ticket already exists for this booking
                var existingTicket = await _ticketRepository.GetByBookingIdAsync(request.BookingId);
                if (existingTicket != null)
                {
                    throw new InvalidOperationException("Ticket already exists for this booking");
                }

                // Generate ticket number
                var ticketNumber = await _ticketRepository.GenerateTicketNumberAsync();

                // Create ticket entity
                var ticket = new Core.Models.Ticket
                {
                    TicketNumber = ticketNumber,
                    UserId = userId,
                    BookingId = request.BookingId,
                    PaymentId = request.PaymentId,
                    BookingNumber = request.BookingNumber,
                    Status = TicketStatus.Generated,
                    Type = request.Type,
                    TransportMode = request.TransportMode,
                    FareType = request.FareType,
                    SourceStationId = request.SourceStationId,
                    SourceStationName = request.SourceStationName,
                    SourceStationCode = request.SourceStationCode,
                    DestinationStationId = request.DestinationStationId,
                    DestinationStationName = request.DestinationStationName,
                    DestinationStationCode = request.DestinationStationCode,
                    RouteId = request.RouteId,
                    RouteName = request.RouteName ?? "",
                    RouteCode = request.RouteCode ?? "",
                    BasePrice = request.BasePrice,
                    DiscountAmount = request.DiscountAmount,
                    TaxAmount = request.TaxAmount,
                    FinalPrice = request.FinalPrice,
                    Currency = request.Currency,
                    ValidFrom = request.ValidFrom,
                    ValidUntil = request.ValidUntil,
                    MaxUsageCount = request.MaxUsageCount,
                    PassengerName = request.PassengerName,
                    PassengerAge = request.PassengerAge,
                    PassengerType = request.PassengerType,
                    PassengerPhone = request.PassengerPhone,
                    PassengerEmail = request.PassengerEmail,
                    AllowsTransfer = request.AllowsTransfer,
                    MaxTransfers = request.MaxTransfers,
                    TransferTimeLimit = request.TransferTimeLimit,
                    ServiceClass = request.ServiceClass,
                    SeatNumber = request.SeatNumber,
                    CoachNumber = request.CoachNumber,
                    SpecialInstructions = request.SpecialInstructions,
                    Metadata = SerializeMetadata(request.Metadata)
                };

                // Generate QR code
                var (qrData, qrImage, qrHash) = await _qrCodeService.GenerateQRCodeAsync(ticket);
                ticket.QRCodeData = qrData;
                ticket.QRCodeImage = qrImage;
                ticket.QRCodeHash = qrHash;

                

                
                

                // Save ticket
                var createdTicket = await _ticketRepository.CreateAsync(ticket);
                // Create QR code record
                var qrCode = new TicketQRCode
                {
                    TicketId = ticket.Id,
                    Ticket = createdTicket,
                    QRCodeId = GenerateQRCodeId(),
                    QRCodeData = qrData,
                    QRCodeHash = qrHash,
                    QRCodeImage = qrImage,
                    Status = QRCodeStatus.Active,
                    GeneratedAt = DateTime.UtcNow,
                    ExpiresAt = ticket.ValidUntil,
                    Size = 300,
                    Format = "PNG",
                    DisplayText = $"Ticket: {ticketNumber}",
                    Instructions = "Show this QR code to the validator"
                };

                var qrCodeRecord = await _qrCodeRepository.CreateAsync(qrCode);
                createdTicket.QRCodes.Add(qrCode);

                // Create history entry
                await CreateHistoryEntryAsync(createdTicket.Id, TicketStatus.Draft, TicketStatus.Generated, "Generated", userId, "Ticket generated successfully");

                // Send notification
                await _notificationService.SendTicketGeneratedNotificationAsync(userId, ticketNumber);

                _logger.LogInformation("Ticket created successfully: {TicketNumber}", ticketNumber);

                return _mapper.Map<TicketResponse>(createdTicket);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating ticket for user {UserId}, booking {BookingId}", userId, request.BookingId);
                throw;
            }
        }

        public async Task<TicketResponse?> GetTicketAsync(Guid ticketId, Guid? userId = null)
        {
            var ticket = await _ticketRepository.GetByIdAsync(ticketId);

            if (ticket == null)
                return null;

            // If userId is provided, ensure the ticket belongs to the user
            if (userId.HasValue && ticket.UserId != userId.Value)
                return null;

            return _mapper.Map<TicketResponse>(ticket);
        }

        public async Task<TicketResponse?> GetTicketByNumberAsync(string ticketNumber, Guid? userId = null)
        {
            var ticket = await _ticketRepository.GetByTicketNumberAsync(ticketNumber);

            if (ticket == null)
                return null;

            // If userId is provided, ensure the ticket belongs to the user
            if (userId.HasValue && ticket.UserId != userId.Value)
                return null;

            return _mapper.Map<TicketResponse>(ticket);
        }

        public async Task<TicketResponse?> GetTicketByBookingIdAsync(Guid bookingId, Guid? userId = null)
        {
            var ticket = await _ticketRepository.GetByBookingIdAsync(bookingId);

            if (ticket == null)
                return null;

            // If userId is provided, ensure the ticket belongs to the user
            if (userId.HasValue && ticket.UserId != userId.Value)
                return null;

            return _mapper.Map<TicketResponse>(ticket);
        }

        public async Task<List<TicketResponse>> GetUserTicketsAsync(Guid userId, int skip = 0, int take = 100)
        {
            var tickets = await _ticketRepository.GetByUserIdAsync(userId, skip, take);
            return _mapper.Map<List<TicketResponse>>(tickets);
        }

        public async Task<List<TicketResponse>> SearchTicketsAsync(TicketSearchRequest request)
        {
            var tickets = await _ticketRepository.SearchAsync(request);
            return _mapper.Map<List<TicketResponse>>(tickets);
        }

        public async Task<TicketResponse> ActivateTicketAsync(Guid ticketId, Guid userId)
        {
            try
            {
                _logger.LogInformation("Activating ticket {TicketId} for user {UserId}", ticketId, userId);

                var ticket = await _ticketRepository.GetByIdAsync(ticketId);
                if (ticket == null)
                {
                    throw new InvalidOperationException("Ticket not found");
                }

                if (ticket.UserId != userId)
                {
                    throw new UnauthorizedAccessException("Access denied");
                }

                if (ticket.Status != TicketStatus.Generated)
                {
                    throw new InvalidOperationException("Ticket cannot be activated in current status");
                }

                if (ticket.ValidFrom > DateTime.UtcNow)
                {
                    throw new InvalidOperationException("Ticket is not yet valid");
                }

                if (ticket.ValidUntil <= DateTime.UtcNow)
                {
                    throw new InvalidOperationException("Ticket has expired");
                }

                var oldStatus = ticket.Status;
                ticket.Status = TicketStatus.Active;
                ticket.UpdatedAt = DateTime.UtcNow;

                await _ticketRepository.UpdateAsync(ticket);
                await CreateHistoryEntryAsync(ticket.Id, oldStatus, TicketStatus.Active, "Activated", userId, "Ticket activated by user");

                _logger.LogInformation("Ticket activated successfully: {TicketNumber}", ticket.TicketNumber);

                return _mapper.Map<TicketResponse>(ticket);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error activating ticket {TicketId}", ticketId);
                throw;
            }
        }

        public async Task<TicketResponse> CancelTicketAsync(Guid ticketId, Guid userId, CancelTicketRequest request)
        {
            try
            {
                _logger.LogInformation("Cancelling ticket {TicketId} for user {UserId}", ticketId, userId);

                var ticket = await _ticketRepository.GetByIdAsync(ticketId);
                if (ticket == null)
                {
                    throw new InvalidOperationException("Ticket not found");
                }

                if (ticket.UserId != userId)
                {
                    throw new UnauthorizedAccessException("Access denied");
                }

                if (ticket.Status == TicketStatus.Cancelled || ticket.Status == TicketStatus.Used)
                {
                    throw new InvalidOperationException("Ticket cannot be cancelled in current status");
                }

                var oldStatus = ticket.Status;
                ticket.Status = TicketStatus.Cancelled;
                ticket.CancellationReason = request.Reason;
                ticket.Notes = request.Notes;
                ticket.UpdatedAt = DateTime.UtcNow;

                await _ticketRepository.UpdateAsync(ticket);
                await CreateHistoryEntryAsync(ticket.Id, oldStatus, TicketStatus.Cancelled, "Cancelled", userId, request.Reason);

                // Process refund if requested
                if (request.RequestRefund && ticket.IsRefundable)
                {
                    await _paymentService.ProcessRefundAsync(ticket.PaymentId, userId, new
                    {
                        Amount = ticket.FinalPrice,
                        Reason = "Ticket cancellation",
                        Notes = request.Notes
                    });
                }

                _logger.LogInformation("Ticket cancelled successfully: {TicketNumber}", ticket.TicketNumber);

                return _mapper.Map<TicketResponse>(ticket);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cancelling ticket {TicketId}", ticketId);
                throw;
            }
        }

        public async Task<TicketValidationResult> ValidateTicketAsync(ValidateTicketRequest request)
        {
            try
            {
                _logger.LogInformation("Validating ticket at station {StationCode} on device {DeviceName}", request.StationCode, request.DeviceName);

                // Decrypt and parse QR code data
                string decryptedData;
                try
                {
                    decryptedData = await _qrCodeService.DecryptQRDataAsync(request.QRCodeData, "encryption_key");
                }
                catch
                {
                    return new TicketValidationResult
                    {
                        IsValid = false,
                        Result = ValidationResult.Invalid,
                        Message = "Invalid QR code format",
                        ValidationTime = DateTime.UtcNow
                    };
                }

                // Find ticket by QR code data
                var ticket = await _ticketRepository.GetByQRCodeDataAsync(request.QRCodeData);
                if (ticket == null)
                {
                    return new TicketValidationResult
                    {
                        IsValid = false,
                        Result = ValidationResult.Invalid,
                        Message = "Ticket not found",
                        ValidationTime = DateTime.UtcNow
                    };
                }

                // Perform validation checks
                var validationResult = await PerformValidationChecks(ticket, request);

                // Create validation record
                var validation = new TicketValidation
                {
                    TicketId = ticket.Id,
                    ValidationId = GenerateValidationId(),
                    ValidationType = request.ValidationType,
                    ValidationResult = validationResult.Result,
                    ValidationTime = DateTime.UtcNow,
                    StationId = request.StationId,
                    StationName = request.StationName,
                    StationCode = request.StationCode,
                    DeviceId = request.DeviceId,
                    DeviceName = request.DeviceName,
                    DeviceType = request.DeviceType,
                    OperatorId = request.OperatorId,
                    OperatorName = request.OperatorName,
                    ValidationMethod = request.ValidationMethod,
                    ValidationError = validationResult.IsValid ? null : validationResult.Message,
                    Latitude = request.Latitude,
                    Longitude = request.Longitude,
                    Direction = request.Direction,
                    Platform = request.Platform,
                    Gate = request.Gate,
                    TripId = request.TripId,
                    VehicleNumber = request.VehicleNumber,
                    RouteNumber = request.RouteNumber
                };

                await _validationRepository.CreateAsync(validation);

                // Update ticket if validation successful
                if (validationResult.IsValid)
                {
                    ticket.UsageCount++;

                    if (ticket.FirstUsedAt == null)
                    {
                        ticket.FirstUsedAt = DateTime.UtcNow;
                        ticket.Status = TicketStatus.Active;
                    }

                    ticket.LastUsedAt = DateTime.UtcNow;

                    if (ticket.UsageCount >= ticket.MaxUsageCount)
                    {
                        ticket.Status = TicketStatus.Used;
                    }

                    await _ticketRepository.UpdateAsync(ticket);
                    await CreateHistoryEntryAsync(ticket.Id, ticket.Status, ticket.Status, "Validated", Guid.Empty, $"Validated at {request.StationName}");
                }

                validationResult.TicketId = ticket.Id;
                validationResult.TicketNumber = ticket.TicketNumber;
                validationResult.TicketStatus = ticket.Status;
                validationResult.TicketType = ticket.Type;
                validationResult.PassengerName = ticket.PassengerName;
                validationResult.ValidUntil = ticket.ValidUntil;
                validationResult.RemainingUsage = ticket.MaxUsageCount - ticket.UsageCount;
                validationResult.AllowsTransfer = ticket.AllowsTransfer;
                validationResult.TransfersRemaining = ticket.MaxTransfers - ticket.TransferCount;
                validationResult.ValidationTime = validation.ValidationTime;
                validationResult.ValidationId = validation.Id;

                _logger.LogInformation("Ticket validation completed: {TicketNumber}, Result: {Result}", ticket.TicketNumber, validationResult.Result);

                return validationResult;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating ticket");
                return new TicketValidationResult
                {
                    IsValid = false,
                    Result = ValidationResult.Invalid,
                    Message = "Validation error occurred",
                    ValidationTime = DateTime.UtcNow
                };
            }
        }

        public async Task<TicketQRCodeResponse> RegenerateQRCodeAsync(Guid ticketId, Guid userId, RegenerateQRRequest request)
        {
            try
            {
                var ticket = await _ticketRepository.GetByIdAsync(ticketId);
                if (ticket == null)
                {
                    throw new InvalidOperationException("Ticket not found");
                }

                if (ticket.UserId != userId)
                {
                    throw new UnauthorizedAccessException("Access denied");
                }

                var newQRCode = await _qrCodeService.RegenerateQRCodeAsync(ticketId, request.Reason ?? "User requested regeneration");

                await CreateHistoryEntryAsync(ticket.Id, ticket.Status, ticket.Status, "QR Regenerated", userId, request.Reason);

                return _mapper.Map<TicketQRCodeResponse>(newQRCode);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error regenerating QR code for ticket {TicketId}", ticketId);
                throw;
            }
        }

        public async Task<byte[]> GetQRCodeImageAsync(Guid ticketId, Guid userId)
        {
            var ticket = await _ticketRepository.GetByIdAsync(ticketId);
            if (ticket == null)
            {
                throw new InvalidOperationException("Ticket not found");
            }

            if (ticket.UserId != userId)
            {
                throw new UnauthorizedAccessException("Access denied");
            }

            return ticket.QRCodeImage ?? Array.Empty<byte>();
        }

        public async Task<TicketStatsResponse> GetStatsAsync(DateTime? fromDate = null, DateTime? toDate = null)
        {
            return await _ticketRepository.GetStatsAsync(fromDate, toDate);
        }

        public async Task<List<TicketResponse>> BulkOperationAsync(BulkTicketOperation operation)
        {
            var results = new List<TicketResponse>();

            foreach (var ticketId in operation.TicketIds)
            {
                try
                {
                    var ticket = await _ticketRepository.GetByIdAsync(ticketId);
                    if (ticket == null) continue;

                    var oldStatus = ticket.Status;

                    switch (operation.Operation.ToLower())
                    {
                        case "cancel":
                            if (ticket.Status != TicketStatus.Cancelled && ticket.Status != TicketStatus.Used)
                            {
                                ticket.Status = TicketStatus.Cancelled;
                                ticket.CancellationReason = operation.Reason;
                                ticket.Notes = operation.Notes;
                            }
                            break;

                        case "activate":
                            if (ticket.Status == TicketStatus.Generated)
                            {
                                ticket.Status = TicketStatus.Active;
                            }
                            break;

                        case "suspend":
                            if (ticket.Status == TicketStatus.Active)
                            {
                                ticket.Status = TicketStatus.Suspended;
                            }
                            break;
                    }

                    if (oldStatus != ticket.Status)
                    {
                        ticket.UpdatedAt = DateTime.UtcNow;
                        await _ticketRepository.UpdateAsync(ticket);
                        await CreateHistoryEntryAsync(ticket.Id, oldStatus, ticket.Status, operation.Operation, Guid.Empty, operation.Reason);
                    }

                    results.Add(_mapper.Map<TicketResponse>(ticket));
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing bulk operation for ticket {TicketId}", ticketId);
                }
            }

            return results;
        }

        public async Task<TicketTransferResponse> ProcessTransferAsync(Guid ticketId, TicketTransferRequest request)
        {
            try
            {
                var ticket = await _ticketRepository.GetByIdAsync(ticketId);
                if (ticket == null)
                {
                    throw new InvalidOperationException("Ticket not found");
                }

                if (!ticket.AllowsTransfer)
                {
                    throw new InvalidOperationException("Ticket does not allow transfers");
                }

                if (ticket.TransferCount >= ticket.MaxTransfers)
                {
                    throw new InvalidOperationException("Maximum transfers exceeded");
                }

                var transfer = new TicketTransfer
                {
                    TicketId = ticket.Id,
                    TransferId = GenerateTransferId(),
                    TransferSequence = ticket.TransferCount + 1,
                    FromStationId = request.FromStationId,
                    FromStationName = await GetStationNameAsync(request.FromStationId),
                    FromStationCode = await GetStationCodeAsync(request.FromStationId),
                    FromTransportMode = request.FromTransportMode,
                    FromRouteNumber = request.FromRouteNumber,
                    FromVehicleNumber = request.FromVehicleNumber,
                    ExitTime = DateTime.UtcNow,
                    ToStationId = request.ToStationId,
                    ToStationName = await GetStationNameAsync(request.ToStationId),
                    ToStationCode = await GetStationCodeAsync(request.ToStationId),
                    ToTransportMode = request.ToTransportMode,
                    ToRouteNumber = request.ToRouteNumber,
                    ToVehicleNumber = request.ToVehicleNumber,
                    EntryTime = DateTime.UtcNow.AddMinutes(5), // Estimated entry time
                    TransferTime = 5, // Default transfer time
                    IsValidTransfer = true,
                    TransferCode = request.TransferCode,
                    TransferReason = request.TransferReason,
                    ExitValidationId = request.ExitValidationId,
                    EntryValidationId = request.EntryValidationId
                };

                ticket.Transfers.Add(transfer);
                ticket.TransferCount++;

                await _ticketRepository.UpdateAsync(ticket);
                await CreateHistoryEntryAsync(ticket.Id, ticket.Status, ticket.Status, "Transfer", Guid.Empty, $"Transfer from {transfer.FromStationName} to {transfer.ToStationName}");

                return _mapper.Map<TicketTransferResponse>(transfer);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing transfer for ticket {TicketId}", ticketId);
                throw;
            }
        }

        public async Task ProcessExpiredTicketsAsync()
        {
            try
            {
                var expiredTickets = await _ticketRepository.GetExpiredTicketsAsync();

                foreach (var ticket in expiredTickets)
                {
                    var oldStatus = ticket.Status;
                    ticket.Status = TicketStatus.Expired;
                    ticket.UpdatedAt = DateTime.UtcNow;

                    await _ticketRepository.UpdateAsync(ticket);
                    await CreateHistoryEntryAsync(ticket.Id, oldStatus, TicketStatus.Expired, "Expired", Guid.Empty, "Ticket expired due to time limit");
                }

                if (expiredTickets.Any())
                {
                    _logger.LogInformation("Processed {Count} expired tickets", expiredTickets.Count());
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing expired tickets");
                throw;
            }
        }

        public async Task<bool> CanUseTicketAsync(Guid ticketId)
        {
            return await _ticketRepository.CanUseTicketAsync(ticketId);
        }

        public async Task<int> GetRemainingUsageAsync(Guid ticketId)
        {
            var ticket = await _ticketRepository.GetByIdAsync(ticketId);
            if (ticket == null) return 0;

            return Math.Max(0, ticket.MaxUsageCount - ticket.UsageCount);
        }

        // Private helper methods
        private async Task<TicketValidationResult> PerformValidationChecks(Core.Models.Ticket ticket, ValidateTicketRequest request)
        {
            // Check ticket status
            if (ticket.Status == TicketStatus.Cancelled)
            {
                return new TicketValidationResult
                {
                    IsValid = false,
                    Result = ValidationResult.Cancelled,
                    Message = "Ticket has been cancelled"
                };
            }

            if (ticket.Status == TicketStatus.Expired)
            {
                return new TicketValidationResult
                {
                    IsValid = false,
                    Result = ValidationResult.Expired,
                    Message = "Ticket has expired"
                };
            }

            if (ticket.Status == TicketStatus.Suspended)
            {
                return new TicketValidationResult
                {
                    IsValid = false,
                    Result = ValidationResult.Suspended,
                    Message = "Ticket is suspended"
                };
            }

            // Check validity period
            if (DateTime.UtcNow < ticket.ValidFrom)
            {
                return new TicketValidationResult
                {
                    IsValid = false,
                    Result = ValidationResult.NotActive,
                    Message = "Ticket is not yet active"
                };
            }

            if (DateTime.UtcNow > ticket.ValidUntil)
            {
                return new TicketValidationResult
                {
                    IsValid = false,
                    Result = ValidationResult.Expired,
                    Message = "Ticket has expired"
                };
            }

            // Check usage limit
            if (ticket.UsageCount >= ticket.MaxUsageCount)
            {
                return new TicketValidationResult
                {
                    IsValid = false,
                    Result = ValidationResult.AlreadyUsed,
                    Message = "Ticket usage limit exceeded"
                };
            }

            // Check station validity (for entry/exit)
            if (request.ValidationType == TicketValidationType.Entry)
            {
                if (ticket.SourceStationId != request.StationId)
                {
                    return new TicketValidationResult
                    {
                        IsValid = false,
                        Result = ValidationResult.OutOfZone,
                        Message = "Invalid entry station"
                    };
                }
            }
            else if (request.ValidationType == TicketValidationType.Exit)
            {
                if (ticket.DestinationStationId != request.StationId)
                {
                    return new TicketValidationResult
                    {
                        IsValid = false,
                        Result = ValidationResult.OutOfZone,
                        Message = "Invalid exit station"
                    };
                }
            }

            return new TicketValidationResult
            {
                IsValid = true,
                Result = ValidationResult.Valid,
                Message = "Ticket is valid"
            };
        }

        private string SerializeMetadata(Dictionary<string, object>? metadata)
        {
            if (metadata == null || !metadata.Any())
                return "{}";

            try
            {
                return System.Text.Json.JsonSerializer.Serialize(metadata);
            }
            catch
            {
                return "{}";
            }
        }

        private string GenerateQRCodeId()
        {
            return $"QR{DateTime.UtcNow:yyyyMMddHHmmss}{Random.Shared.Next(1000, 9999)}";
        }

        private string GenerateValidationId()
        {
            return $"VAL{DateTime.UtcNow:yyyyMMddHHmmss}{Random.Shared.Next(1000, 9999)}";
        }

        private string GenerateTransferId()
        {
            return $"TRF{DateTime.UtcNow:yyyyMMddHHmmss}{Random.Shared.Next(1000, 9999)}";
        }

        private async Task CreateHistoryEntryAsync(Guid ticketId, TicketStatus fromStatus, TicketStatus toStatus, string action, Guid actionBy, string? reason = null)
        {
            var history = new TicketHistory
            {
                TicketId = ticketId,
                FromStatus = fromStatus,
                ToStatus = toStatus,
                Action = action,
                Reason = reason,
                ActionBy = actionBy,
                ActionByType = actionBy == Guid.Empty ? "System" : "User",
                ActionTime = DateTime.UtcNow
            };

            await _historyRepository.CreateAsync(history);
        }

        private async Task<string> GetStationNameAsync(Guid stationId)
        {
            // TODO: Implement station service call
            return "Station Name";
        }

        private async Task<string> GetStationCodeAsync(Guid stationId)
        {
            // TODO: Implement station service call
            return "STN";
        }
    }
}