using Microsoft.EntityFrameworkCore;
using TransitAFC.Services.Ticket.Core.Models;

namespace TransitAFC.Services.Ticket.Infrastructure.Repositories
{
    public class TicketHistoryRepository : ITicketHistoryRepository
    {
        private readonly TicketDbContext _context;

        public TicketHistoryRepository(TicketDbContext context)
        {
            _context = context;
        }

        public async Task<TicketHistory> CreateAsync(TicketHistory history)
        {
            _context.TicketHistory.Add(history);
            await _context.SaveChangesAsync();
            return history;
        }

        public async Task<IEnumerable<TicketHistory>> GetByTicketIdAsync(Guid ticketId)
        {
            return await _context.TicketHistory
                .Where(h => h.TicketId == ticketId && !h.IsDeleted)
                .OrderByDescending(h => h.ActionTime)
                .ToListAsync();
        }

        public async Task<IEnumerable<TicketHistory>> GetByUserIdAsync(Guid userId, DateTime? fromDate = null, DateTime? toDate = null)
        {
            var query = _context.TicketHistory
                .Include(h => h.Ticket)
                .Where(h => h.ActionBy == userId && !h.IsDeleted);

            if (fromDate.HasValue)
                query = query.Where(h => h.ActionTime >= fromDate);

            if (toDate.HasValue)
                query = query.Where(h => h.ActionTime <= toDate);

            return await query.OrderByDescending(h => h.ActionTime).ToListAsync();
        }

        public async Task<IEnumerable<TicketHistory>> GetHistoryAsync(DateTime fromDate, DateTime toDate)
        {
            return await _context.TicketHistory
                .Include(h => h.Ticket)
                .Where(h => h.ActionTime >= fromDate && h.ActionTime <= toDate && !h.IsDeleted)
                .OrderByDescending(h => h.ActionTime)
                .ToListAsync();
        }
    }
}