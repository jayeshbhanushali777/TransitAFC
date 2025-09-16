using Microsoft.EntityFrameworkCore;
using TransitAFC.Services.Booking.Core.Models;
using TransitAFC.Services.Booking.Infrastructure.Repositories;

namespace TransitAFC.Services.Booking.Infrastructure.Repositories
{
    public class BookingHistoryRepository : IBookingHistoryRepository
    {
        private readonly BookingDbContext _context;

        public BookingHistoryRepository(BookingDbContext context)
        {
            _context = context;
        }

        public async Task<BookingHistory> CreateAsync(BookingHistory history)
        {
            _context.BookingHistory.Add(history);
            await _context.SaveChangesAsync();
            return history;
        }

        public async Task<IEnumerable<BookingHistory>> GetByBookingIdAsync(Guid bookingId)
        {
            return await _context.BookingHistory
                .Where(h => h.BookingId == bookingId)
                .OrderByDescending(h => h.ActionTime)
                .ToListAsync();
        }

        public async Task<IEnumerable<BookingHistory>> GetByUserIdAsync(Guid userId, int skip = 0, int take = 100)
        {
            return await _context.BookingHistory
                .Include(h => h.Booking)
                .Where(h => h.Booking.UserId == userId)
                .OrderByDescending(h => h.ActionTime)
                .Skip(skip)
                .Take(take)
                .ToListAsync();
        }
    }
}