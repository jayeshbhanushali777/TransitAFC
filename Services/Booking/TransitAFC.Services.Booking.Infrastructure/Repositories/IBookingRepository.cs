using TransitAFC.Services.Booking.Core.DTOs;
using TransitAFC.Services.Booking.Core.Models;

namespace TransitAFC.Services.Booking.Infrastructure.Repositories
{
    public interface IBookingRepository
    {
        Task<Core.Models.Booking?> GetByIdAsync(Guid id);
        Task<Core.Models.Booking?> GetByBookingNumberAsync(string bookingNumber);
        Task<IEnumerable<Core.Models.Booking>> GetByUserIdAsync(Guid userId, int skip = 0, int take = 100);
        Task<IEnumerable<Core.Models.Booking>> SearchAsync(BookingSearchRequest request);
        Task<Core.Models.Booking> CreateAsync(Core.Models.Booking booking);
        Task<Core.Models.Booking> UpdateAsync(Core.Models.Booking booking);
        Task<bool> DeleteAsync(Guid id);
        Task<bool> ExistsAsync(string bookingNumber);
        Task<string> GenerateBookingNumberAsync();
        Task<BookingStatsResponse> GetStatsAsync(DateTime? fromDate = null, DateTime? toDate = null);
        Task<IEnumerable<Core.Models.Booking>> GetExpiredBookingsAsync();
        Task<IEnumerable<Core.Models.Booking>> GetBookingsByRouteAsync(Guid routeId, DateTime date);
        Task<int> GetBookingCountByUserAsync(Guid userId, DateTime? fromDate = null);
    }
}