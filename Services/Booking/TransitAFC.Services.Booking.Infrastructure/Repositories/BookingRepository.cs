using Microsoft.EntityFrameworkCore;
using TransitAFC.Services.Booking.Core.DTOs;
using TransitAFC.Services.Booking.Core.Models;
using TransitAFC.Services.Booking.Infrastructure.Repositories;

namespace TransitAFC.Services.Booking.Infrastructure.Repositories
{
    public class BookingRepository : IBookingRepository
    {
        private readonly BookingDbContext _context;

        public BookingRepository(BookingDbContext context)
        {
            _context = context;
        }

        public async Task<Core.Models.Booking?> GetByIdAsync(Guid id)
        {
            return await _context.Bookings
                .Include(b => b.Passengers)
                .Include(b => b.BookingHistory)
                .FirstOrDefaultAsync(b => b.Id == id && !b.IsDeleted);
        }

        public async Task<Core.Models.Booking?> GetByBookingNumberAsync(string bookingNumber)
        {
            return await _context.Bookings
                .Include(b => b.Passengers)
                .Include(b => b.BookingHistory)
                .FirstOrDefaultAsync(b => b.BookingNumber == bookingNumber && !b.IsDeleted);
        }

        public async Task<IEnumerable<Core.Models.Booking>> GetByUserIdAsync(Guid userId, int skip = 0, int take = 100)
        {
            return await _context.Bookings
                .Include(b => b.Passengers)
                .Where(b => b.UserId == userId && !b.IsDeleted)
                .OrderByDescending(b => b.CreatedAt)
                .Skip(skip)
                .Take(take)
                .ToListAsync();
        }

        public async Task<IEnumerable<Core.Models.Booking>> SearchAsync(BookingSearchRequest request)
        {
            var query = _context.Bookings
                .Include(b => b.Passengers)
                .Where(b => !b.IsDeleted);

            if (request.UserId.HasValue)
                query = query.Where(b => b.UserId == request.UserId);

            if (!string.IsNullOrEmpty(request.BookingNumber))
                query = query.Where(b => b.BookingNumber.Contains(request.BookingNumber));

            if (request.Status.HasValue)
                query = query.Where(b => b.Status == request.Status);

            if (request.FromDate.HasValue)
                query = query.Where(b => b.CreatedAt >= request.FromDate);

            if (request.ToDate.HasValue)
                query = query.Where(b => b.CreatedAt <= request.ToDate);

            if (request.RouteId.HasValue)
                query = query.Where(b => b.RouteId == request.RouteId);

            if (!string.IsNullOrEmpty(request.ContactEmail))
                query = query.Where(b => b.ContactEmail.Contains(request.ContactEmail));

            if (!string.IsNullOrEmpty(request.ContactPhone))
                query = query.Where(b => b.ContactPhone.Contains(request.ContactPhone));

            return await query
                .OrderByDescending(b => b.CreatedAt)
                .Skip(request.Skip)
                .Take(request.Take)
                .ToListAsync();
        }

        public async Task<Core.Models.Booking> CreateAsync(Core.Models.Booking booking)
        {
            _context.Bookings.Add(booking);
            await _context.SaveChangesAsync();
            return booking;
        }

        public async Task<Core.Models.Booking> UpdateAsync(Core.Models.Booking booking)
        {
            booking.UpdatedAt = DateTime.UtcNow;
            _context.Bookings.Update(booking);
            await _context.SaveChangesAsync();
            return booking;
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var booking = await GetByIdAsync(id);
            if (booking == null) return false;

            booking.IsDeleted = true;
            booking.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ExistsAsync(string bookingNumber)
        {
            return await _context.Bookings
                .AnyAsync(b => b.BookingNumber == bookingNumber && !b.IsDeleted);
        }

        public async Task<string> GenerateBookingNumberAsync()
        {
            string prefix = "BK";
            string datePart = DateTime.UtcNow.ToString("yyyyMMdd");

            // Get the last booking number for today
            var lastBooking = await _context.Bookings
                .Where(b => b.BookingNumber.StartsWith(prefix + datePart))
                .OrderByDescending(b => b.BookingNumber)
                .FirstOrDefaultAsync();

            int sequence = 1;
            if (lastBooking != null)
            {
                var lastSequence = lastBooking.BookingNumber.Substring(prefix.Length + datePart.Length);
                if (int.TryParse(lastSequence, out int lastSeq))
                {
                    sequence = lastSeq + 1;
                }
            }

            return $"{prefix}{datePart}{sequence:D4}";
        }

        public async Task<BookingStatsResponse> GetStatsAsync(DateTime? fromDate = null, DateTime? toDate = null)
        {
            var query = _context.Bookings.Where(b => !b.IsDeleted);

            if (fromDate.HasValue)
                query = query.Where(b => b.CreatedAt >= fromDate);

            if (toDate.HasValue)
                query = query.Where(b => b.CreatedAt <= toDate);

            var bookings = await query.ToListAsync();

            return new BookingStatsResponse
            {
                TotalBookings = bookings.Count,
                PendingBookings = bookings.Count(b => b.Status == BookingStatus.Pending),
                ConfirmedBookings = bookings.Count(b => b.Status == BookingStatus.Confirmed),
                CancelledBookings = bookings.Count(b => b.Status == BookingStatus.Cancelled),
                CompletedBookings = bookings.Count(b => b.Status == BookingStatus.Completed),
                TotalRevenue = bookings.Where(b => b.Status == BookingStatus.Confirmed || b.Status == BookingStatus.Completed).Sum(b => b.FinalAmount),
                PendingRevenue = bookings.Where(b => b.Status == BookingStatus.Pending).Sum(b => b.FinalAmount),
                TotalPassengers = bookings.Sum(b => b.PassengerCount),
                BookingsByRoute = bookings.GroupBy(b => b.RouteName).ToDictionary(g => g.Key, g => g.Count()),
                BookingsByStatus = bookings.GroupBy(b => b.Status.ToString()).ToDictionary(g => g.Key, g => g.Count()),
                RevenueByDate = bookings
                    .Where(b => b.Status == BookingStatus.Confirmed || b.Status == BookingStatus.Completed)
                    .GroupBy(b => b.CreatedAt.Date.ToString("yyyy-MM-dd"))
                    .ToDictionary(g => g.Key, g => g.Sum(b => b.FinalAmount))
            };
        }

        public async Task<IEnumerable<Core.Models.Booking>> GetExpiredBookingsAsync()
        {
            return await _context.Bookings
                .Where(b => b.Status == BookingStatus.Pending &&
                           b.BookingExpiresAt.HasValue &&
                           b.BookingExpiresAt < DateTime.UtcNow &&
                           !b.IsDeleted)
                .ToListAsync();
        }

        public async Task<IEnumerable<Core.Models.Booking>> GetBookingsByRouteAsync(Guid routeId, DateTime date)
        {
            var startDate = date.Date;
            var endDate = startDate.AddDays(1);

            return await _context.Bookings
                .Include(b => b.Passengers)
                .Where(b => b.RouteId == routeId &&
                           b.DepartureTime >= startDate &&
                           b.DepartureTime < endDate &&
                           (b.Status == BookingStatus.Confirmed || b.Status == BookingStatus.Completed) &&
                           !b.IsDeleted)
                .OrderBy(b => b.DepartureTime)
                .ToListAsync();
        }

        public async Task<int> GetBookingCountByUserAsync(Guid userId, DateTime? fromDate = null)
        {
            var query = _context.Bookings.Where(b => b.UserId == userId && !b.IsDeleted);

            if (fromDate.HasValue)
                query = query.Where(b => b.CreatedAt >= fromDate);

            return await query.CountAsync();
        }
    }
}