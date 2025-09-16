using TransitAFC.Services.Booking.Core.DTOs;

namespace TransitAFC.Services.Booking.API.Services
{
    public interface IBookingService
    {
        Task<BookingResponse> CreateBookingAsync(Guid userId, CreateBookingRequest request);
        Task<BookingResponse?> GetBookingAsync(Guid bookingId, Guid? userId = null);
        Task<BookingResponse?> GetBookingByNumberAsync(string bookingNumber, Guid? userId = null);
        Task<List<BookingResponse>> GetUserBookingsAsync(Guid userId, int skip = 0, int take = 100);
        Task<List<BookingResponse>> SearchBookingsAsync(BookingSearchRequest request);
        Task<BookingResponse> UpdateBookingAsync(Guid bookingId, Guid userId, UpdateBookingRequest request);
        Task<BookingResponse> ConfirmBookingAsync(Guid bookingId, Guid userId, ConfirmBookingRequest request);
        Task<BookingResponse> CancelBookingAsync(Guid bookingId, Guid userId, CancelBookingRequest request);
        Task<FareCalculationResponse> CalculateFareAsync(FareCalculationRequest request);
        Task<BookingStatsResponse> GetStatsAsync(DateTime? fromDate = null, DateTime? toDate = null);
        Task ProcessExpiredBookingsAsync();
    }
}