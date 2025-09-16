using AutoMapper;
using System.Security.Claims;
using TransitAFC.Services.Booking.API.Services;
using TransitAFC.Services.Booking.Core.DTOs;
using TransitAFC.Services.Booking.Core.Models;
using TransitAFC.Services.Booking.Infrastructure.Repositories;

namespace TransitAFC.Services.Booking.API.Services
{
    public class BookingService : IBookingService
    {
        private readonly IBookingRepository _bookingRepository;
        private readonly IBookingHistoryRepository _historyRepository;
        private readonly IRouteService _routeService;
        private readonly IFareCalculationService _fareCalculationService;
        private readonly IMapper _mapper;
        private readonly ILogger<BookingService> _logger;

        public BookingService(
            IBookingRepository bookingRepository,
            IBookingHistoryRepository historyRepository,
            IRouteService routeService,
            IFareCalculationService fareCalculationService,
            IMapper mapper,
            ILogger<BookingService> logger)
        {
            _bookingRepository = bookingRepository;
            _historyRepository = historyRepository;
            _routeService = routeService;
            _fareCalculationService = fareCalculationService;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<BookingResponse> CreateBookingAsync(Guid userId, CreateBookingRequest request)
        {
            try
            {
                _logger.LogInformation("Creating booking for user {UserId}", userId);

                // Validate route and stations
                var routeInfo = await _routeService.GetRouteInfoAsync(request.RouteId, request.SourceStationId, request.DestinationStationId);
                if (routeInfo == null)
                {
                    throw new InvalidOperationException("Invalid route or stations");
                }

                // Calculate fare
                var fareRequest = new FareCalculationRequest
                {
                    RouteId = request.RouteId,
                    SourceStationId = request.SourceStationId,
                    DestinationStationId = request.DestinationStationId,
                    PassengerTypes = request.Passengers.Select(p => p.PassengerType).ToList(),
                    DiscountCode = request.DiscountCode,
                    TravelDate = request.DepartureTime
                };

                var fareCalculation = await _fareCalculationService.CalculateFareAsync(fareRequest);

                // Generate booking number
                var bookingNumber = await _bookingRepository.GenerateBookingNumberAsync();

                // Create booking entity
                var booking = new Core.Models.Booking
                {
                    BookingNumber = bookingNumber,
                    UserId = userId,
                    RouteId = request.RouteId,
                    RouteCode = routeInfo.RouteCode,
                    RouteName = routeInfo.RouteName,
                    SourceStationId = request.SourceStationId,
                    SourceStationName = routeInfo.SourceStationName,
                    DestinationStationId = request.DestinationStationId,
                    DestinationStationName = routeInfo.DestinationStationName,
                    DepartureTime = request.DepartureTime,
                    ArrivalTime = request.DepartureTime.Add(routeInfo.EstimatedDuration),
                    Status = BookingStatus.Draft,
                    PassengerCount = request.Passengers.Count,
                    TotalFare = fareCalculation.TotalFare,
                    DiscountAmount = fareCalculation.DiscountAmount,
                    TaxAmount = fareCalculation.TaxAmount,
                    FinalAmount = fareCalculation.FinalAmount,
                    DiscountCode = request.DiscountCode,
                    ContactEmail = request.ContactEmail,
                    ContactPhone = request.ContactPhone,
                    SpecialRequests = request.SpecialRequests,
                    BookingExpiresAt = DateTime.UtcNow.AddMinutes(15), // 15 minutes to complete booking
                    BookingSource = request.BookingSource ?? "WebApp"
                };

                // Add passengers
                for (int i = 0; i < request.Passengers.Count; i++)
                {
                    var passengerRequest = request.Passengers[i];
                    var passengerFare = fareCalculation.PassengerFares[i];

                    var passenger = new BookingPassenger
                    {
                        FirstName = passengerRequest.FirstName,
                        LastName = passengerRequest.LastName,
                        PassengerType = passengerRequest.PassengerType,
                        Age = passengerRequest.Age,
                        Gender = passengerRequest.Gender,
                        SeatType = passengerRequest.PreferredSeatType,
                        Fare = passengerFare.BaseFare,
                        DiscountAmount = passengerFare.DiscountAmount,
                        FinalFare = passengerFare.FinalFare,
                        IdentityType = passengerRequest.IdentityType,
                        IdentityNumber = passengerRequest.IdentityNumber,
                        ContactPhone = passengerRequest.ContactPhone,
                        ContactEmail = passengerRequest.ContactEmail,
                        SpecialRequests = passengerRequest.SpecialRequests,
                        HasWheelchairAccess = passengerRequest.HasWheelchairAccess,
                        IsPrimaryPassenger = passengerRequest.IsPrimaryPassenger
                    };

                    booking.Passengers.Add(passenger);
                }

                // Save booking
                var createdBooking = await _bookingRepository.CreateAsync(booking);

                // Create history entry
                await CreateHistoryEntryAsync(createdBooking.Id, BookingStatus.Draft, BookingStatus.Draft, "Created", userId, "Booking created");

                // Update status to Pending
                createdBooking.Status = BookingStatus.Pending;
                await _bookingRepository.UpdateAsync(createdBooking);

                await CreateHistoryEntryAsync(createdBooking.Id, BookingStatus.Draft, BookingStatus.Pending, "Status Updated", userId, "Booking moved to pending status");

                _logger.LogInformation("Booking created successfully: {BookingNumber}", bookingNumber);

                return _mapper.Map<BookingResponse>(createdBooking);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating booking for user {UserId}", userId);
                throw;
            }
        }

        public async Task<BookingResponse?> GetBookingAsync(Guid bookingId, Guid? userId = null)
        {
            var booking = await _bookingRepository.GetByIdAsync(bookingId);

            if (booking == null)
                return null;

            // If userId is provided, ensure the booking belongs to the user
            if (userId.HasValue && booking.UserId != userId.Value)
                return null;

            return _mapper.Map<BookingResponse>(booking);
        }

        public async Task<BookingResponse?> GetBookingByNumberAsync(string bookingNumber, Guid? userId = null)
        {
            var booking = await _bookingRepository.GetByBookingNumberAsync(bookingNumber);

            if (booking == null)
                return null;

            // If userId is provided, ensure the booking belongs to the user
            if (userId.HasValue && booking.UserId != userId.Value)
                return null;

            return _mapper.Map<BookingResponse>(booking);
        }

        public async Task<List<BookingResponse>> GetUserBookingsAsync(Guid userId, int skip = 0, int take = 100)
        {
            var bookings = await _bookingRepository.GetByUserIdAsync(userId, skip, take);
            return _mapper.Map<List<BookingResponse>>(bookings);
        }

        public async Task<List<BookingResponse>> SearchBookingsAsync(BookingSearchRequest request)
        {
            var bookings = await _bookingRepository.SearchAsync(request);
            return _mapper.Map<List<BookingResponse>>(bookings);
        }

        public async Task<BookingResponse> UpdateBookingAsync(Guid bookingId, Guid userId, UpdateBookingRequest request)
        {
            try
            {
                var booking = await _bookingRepository.GetByIdAsync(bookingId);
                if (booking == null)
                {
                    throw new InvalidOperationException("Booking not found");
                }

                if (booking.UserId != userId)
                {
                    throw new UnauthorizedAccessException("Access denied");
                }

                if (booking.Status != BookingStatus.Pending && booking.Status != BookingStatus.Draft)
                {
                    throw new InvalidOperationException("Booking cannot be updated in current status");
                }

                var oldStatus = booking.Status;

                // Update booking details
                if (!string.IsNullOrEmpty(request.ContactEmail))
                    booking.ContactEmail = request.ContactEmail;

                if (!string.IsNullOrEmpty(request.ContactPhone))
                    booking.ContactPhone = request.ContactPhone;

                if (!string.IsNullOrEmpty(request.SpecialRequests))
                    booking.SpecialRequests = request.SpecialRequests;

                // Update passengers if provided
                if (request.Passengers != null && request.Passengers.Any())
                {
                    foreach (var passengerUpdate in request.Passengers)
                    {
                        var passenger = booking.Passengers.FirstOrDefault(p => p.Id == passengerUpdate.PassengerId);
                        if (passenger != null)
                        {
                            if (!string.IsNullOrEmpty(passengerUpdate.ContactPhone))
                                passenger.ContactPhone = passengerUpdate.ContactPhone;

                            if (!string.IsNullOrEmpty(passengerUpdate.ContactEmail))
                                passenger.ContactEmail = passengerUpdate.ContactEmail;

                            if (!string.IsNullOrEmpty(passengerUpdate.SpecialRequests))
                                passenger.SpecialRequests = passengerUpdate.SpecialRequests;

                            if (!string.IsNullOrEmpty(passengerUpdate.SeatNumber))
                                passenger.SeatNumber = passengerUpdate.SeatNumber;
                        }
                    }
                }

                var updatedBooking = await _bookingRepository.UpdateAsync(booking);

                await CreateHistoryEntryAsync(bookingId, oldStatus, booking.Status, "Updated", userId, "Booking details updated");

                _logger.LogInformation("Booking updated successfully: {BookingId}", bookingId);

                return _mapper.Map<BookingResponse>(updatedBooking);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating booking {BookingId} for user {UserId}", bookingId, userId);
                throw;
            }
        }

        public async Task<BookingResponse> ConfirmBookingAsync(Guid bookingId, Guid userId, ConfirmBookingRequest request)
        {
            try
            {
                var booking = await _bookingRepository.GetByIdAsync(bookingId);
                if (booking == null)
                {
                    throw new InvalidOperationException("Booking not found");
                }

                if (booking.UserId != userId)
                {
                    throw new UnauthorizedAccessException("Access denied");
                }

                if (booking.Status != BookingStatus.Pending)
                {
                    throw new InvalidOperationException("Booking cannot be confirmed in current status");
                }

                // Verify payment (this would typically call Payment Service)
                // For now, we'll assume payment is valid
                booking.PaymentId = request.PaymentId;
                booking.Status = BookingStatus.Confirmed;
                booking.ConfirmedAt = DateTime.UtcNow;
                booking.VehicleNumber = request.VehicleNumber;

                // Assign seats if provided
                if (request.SeatAssignments != null && request.SeatAssignments.Any())
                {
                    var seatNumbers = new List<string>();

                    foreach (var seatAssignment in request.SeatAssignments)
                    {
                        var passenger = booking.Passengers.FirstOrDefault(p => p.Id == seatAssignment.PassengerId);
                        if (passenger != null)
                        {
                            passenger.SeatNumber = seatAssignment.SeatNumber;
                            passenger.SeatType = seatAssignment.SeatType;
                            seatNumbers.Add(seatAssignment.SeatNumber);
                        }
                    }

                    booking.SeatNumbers = System.Text.Json.JsonSerializer.Serialize(seatNumbers);
                }

                var updatedBooking = await _bookingRepository.UpdateAsync(booking);

                await CreateHistoryEntryAsync(bookingId, BookingStatus.Pending, BookingStatus.Confirmed, "Confirmed", userId, "Booking confirmed with payment");

                _logger.LogInformation("Booking confirmed successfully: {BookingId}", bookingId);

                return _mapper.Map<BookingResponse>(updatedBooking);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error confirming booking {BookingId} for user {UserId}", bookingId, userId);
                throw;
            }
        }

        public async Task<BookingResponse> CancelBookingAsync(Guid bookingId, Guid userId, CancelBookingRequest request)
        {
            try
            {
                var booking = await _bookingRepository.GetByIdAsync(bookingId);
                if (booking == null)
                {
                    throw new InvalidOperationException("Booking not found");
                }

                if (booking.UserId != userId)
                {
                    throw new UnauthorizedAccessException("Access denied");
                }

                if (booking.Status == BookingStatus.Cancelled || booking.Status == BookingStatus.Completed)
                {
                    throw new InvalidOperationException("Booking cannot be cancelled in current status");
                }

                var oldStatus = booking.Status;
                booking.Status = BookingStatus.Cancelled;
                booking.CancelledAt = DateTime.UtcNow;
                booking.CancellationReason = request.Reason;

                var updatedBooking = await _bookingRepository.UpdateAsync(booking);

                await CreateHistoryEntryAsync(bookingId, oldStatus, BookingStatus.Cancelled, "Cancelled", userId, request.Reason);

                _logger.LogInformation("Booking cancelled successfully: {BookingId}", bookingId);

                return _mapper.Map<BookingResponse>(updatedBooking);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cancelling booking {BookingId} for user {UserId}", bookingId, userId);
                throw;
            }
        }

        public async Task<FareCalculationResponse> CalculateFareAsync(FareCalculationRequest request)
        {
            return await _fareCalculationService.CalculateFareAsync(request);
        }

        public async Task<BookingStatsResponse> GetStatsAsync(DateTime? fromDate = null, DateTime? toDate = null)
        {
            return await _bookingRepository.GetStatsAsync(fromDate, toDate);
        }

        public async Task ProcessExpiredBookingsAsync()
        {
            try
            {
                var expiredBookings = await _bookingRepository.GetExpiredBookingsAsync();

                foreach (var booking in expiredBookings)
                {
                    booking.Status = BookingStatus.Failed;
                    booking.CancellationReason = "Booking expired";
                    await _bookingRepository.UpdateAsync(booking);

                    await CreateHistoryEntryAsync(booking.Id, BookingStatus.Pending, BookingStatus.Failed, "Expired", Guid.Empty, "Booking expired due to timeout");
                }

                if (expiredBookings.Any())
                {
                    _logger.LogInformation("Processed {Count} expired bookings", expiredBookings.Count());
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing expired bookings");
                throw;
            }
        }

        private async Task CreateHistoryEntryAsync(Guid bookingId, BookingStatus fromStatus, BookingStatus toStatus, string action, Guid actionBy, string? reason = null)
        {
            var history = new BookingHistory
            {
                BookingId = bookingId,
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
    }
}