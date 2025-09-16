using TransitAFC.Services.Ticket.Core.DTOs;
using TransitAFC.Services.Ticket.Core.Models;

namespace TransitAFC.Services.Ticket.Infrastructure.Repositories
{
    public interface ITicketRepository
    {
        Task<Core.Models.Ticket?> GetByIdAsync(Guid id);
        Task<Core.Models.Ticket?> GetByTicketNumberAsync(string ticketNumber);
        Task<Core.Models.Ticket?> GetByBookingIdAsync(Guid bookingId);
        Task<Core.Models.Ticket?> GetByQRCodeDataAsync(string qrCodeData);
        Task<IEnumerable<Core.Models.Ticket>> GetByUserIdAsync(Guid userId, int skip = 0, int take = 100);
        Task<IEnumerable<Core.Models.Ticket>> SearchAsync(TicketSearchRequest request);
        Task<Core.Models.Ticket> CreateAsync(Core.Models.Ticket ticket);
        Task<Core.Models.Ticket> UpdateAsync(Core.Models.Ticket ticket);
        Task<bool> DeleteAsync(Guid id);
        Task<bool> ExistsAsync(string ticketNumber);
        Task<string> GenerateTicketNumberAsync();
        Task<TicketStatsResponse> GetStatsAsync(DateTime? fromDate = null, DateTime? toDate = null);
        Task<IEnumerable<Core.Models.Ticket>> GetExpiredTicketsAsync();
        Task<IEnumerable<Core.Models.Ticket>> GetActiveTicketsAsync();
        Task<IEnumerable<Core.Models.Ticket>> GetTicketsByStationAsync(Guid stationId, DateTime? date = null);
        Task<int> GetUsageCountAsync(Guid ticketId);
        Task<bool> CanUseTicketAsync(Guid ticketId);
    }
}