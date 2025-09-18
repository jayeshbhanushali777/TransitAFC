using TransitAFC.Services.Ticket.Core.Models;

namespace TransitAFC.Services.Ticket.Infrastructure.Repositories
{
    public interface ITicketHistoryRepository
    {
        Task<TicketHistory> CreateAsync(TicketHistory history);
        Task<IEnumerable<TicketHistory>> GetByTicketIdAsync(Guid ticketId);
        Task<IEnumerable<TicketHistory>> GetByUserIdAsync(Guid userId, DateTime? fromDate = null, DateTime? toDate = null);
        Task<IEnumerable<TicketHistory>> GetHistoryAsync(DateTime fromDate, DateTime toDate);
    }
}