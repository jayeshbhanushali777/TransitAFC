using TransitAFC.Services.Ticket.Core.Models;

namespace TransitAFC.Services.Ticket.Infrastructure.Repositories
{
    public interface ITicketValidationRepository
    {
        Task<TicketValidation> CreateAsync(TicketValidation validation);
        Task<TicketValidation?> GetByIdAsync(Guid id);
        Task<TicketValidation?> GetByValidationIdAsync(string validationId);
        Task<IEnumerable<TicketValidation>> GetByTicketIdAsync(Guid ticketId);
        Task<IEnumerable<TicketValidation>> GetByStationIdAsync(Guid stationId, DateTime? date = null);
        Task<IEnumerable<TicketValidation>> GetByDeviceIdAsync(Guid deviceId, DateTime? date = null);
        Task<IEnumerable<TicketValidation>> GetValidationsAsync(DateTime fromDate, DateTime toDate);
        Task<int> GetValidationCountAsync(Guid ticketId);
        Task<bool> HasValidEntryAsync(Guid ticketId);
        Task<TicketValidation?> GetLastValidationAsync(Guid ticketId);
    }
}