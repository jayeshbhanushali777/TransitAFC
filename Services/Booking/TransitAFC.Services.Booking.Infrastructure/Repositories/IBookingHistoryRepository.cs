using TransitAFC.Services.Booking.Core.Models;

namespace TransitAFC.Services.Booking.Infrastructure.Repositories
{
    public interface IBookingHistoryRepository
    {
        Task<BookingHistory> CreateAsync(BookingHistory history);
        Task<IEnumerable<BookingHistory>> GetByBookingIdAsync(Guid bookingId);
        Task<IEnumerable<BookingHistory>> GetByUserIdAsync(Guid userId, int skip = 0, int take = 100);
    }
}