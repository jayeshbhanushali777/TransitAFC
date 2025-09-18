using Microsoft.EntityFrameworkCore;
using TransitAFC.Services.Ticket.Core.Models;

namespace TransitAFC.Services.Ticket.Infrastructure.Repositories
{
    public class TicketValidationRepository : ITicketValidationRepository
    {
        private readonly TicketDbContext _context;

        public TicketValidationRepository(TicketDbContext context)
        {
            _context = context;
        }

        public async Task<TicketValidation> CreateAsync(TicketValidation validation)
        {
            _context.TicketValidations.Add(validation);
            await _context.SaveChangesAsync();
            return validation;
        }

        public async Task<TicketValidation?> GetByIdAsync(Guid id)
        {
            return await _context.TicketValidations
                .Include(v => v.Ticket)
                .FirstOrDefaultAsync(v => v.Id == id && !v.IsDeleted);
        }

        public async Task<TicketValidation?> GetByValidationIdAsync(string validationId)
        {
            return await _context.TicketValidations
                .Include(v => v.Ticket)
                .FirstOrDefaultAsync(v => v.ValidationId == validationId && !v.IsDeleted);
        }

        public async Task<IEnumerable<TicketValidation>> GetByTicketIdAsync(Guid ticketId)
        {
            return await _context.TicketValidations
                .Where(v => v.TicketId == ticketId && !v.IsDeleted)
                .OrderByDescending(v => v.ValidationTime)
                .ToListAsync();
        }

        public async Task<IEnumerable<TicketValidation>> GetByStationIdAsync(Guid stationId, DateTime? date = null)
        {
            var query = _context.TicketValidations
                .Include(v => v.Ticket)
                .Where(v => v.StationId == stationId && !v.IsDeleted);

            if (date.HasValue)
            {
                var startDate = date.Value.Date;
                var endDate = startDate.AddDays(1);
                query = query.Where(v => v.ValidationTime >= startDate && v.ValidationTime < endDate);
            }

            return await query.OrderByDescending(v => v.ValidationTime).ToListAsync();
        }

        public async Task<IEnumerable<TicketValidation>> GetByDeviceIdAsync(Guid deviceId, DateTime? date = null)
        {
            var query = _context.TicketValidations
                .Include(v => v.Ticket)
                .Where(v => v.DeviceId == deviceId && !v.IsDeleted);

            if (date.HasValue)
            {
                var startDate = date.Value.Date;
                var endDate = startDate.AddDays(1);
                query = query.Where(v => v.ValidationTime >= startDate && v.ValidationTime < endDate);
            }

            return await query.OrderByDescending(v => v.ValidationTime).ToListAsync();
        }

        public async Task<IEnumerable<TicketValidation>> GetValidationsAsync(DateTime fromDate, DateTime toDate)
        {
            return await _context.TicketValidations
                .Include(v => v.Ticket)
                .Where(v => v.ValidationTime >= fromDate && v.ValidationTime <= toDate && !v.IsDeleted)
                .OrderByDescending(v => v.ValidationTime)
                .ToListAsync();
        }

        public async Task<int> GetValidationCountAsync(Guid ticketId)
        {
            return await _context.TicketValidations
                .Where(v => v.TicketId == ticketId && v.ValidationResult == ValidationResult.Valid && !v.IsDeleted)
                .CountAsync();
        }

        public async Task<bool> HasValidEntryAsync(Guid ticketId)
        {
            return await _context.TicketValidations
                .AnyAsync(v => v.TicketId == ticketId &&
                              v.ValidationType == TicketValidationType.Entry &&
                              v.ValidationResult == ValidationResult.Valid &&
                              !v.IsDeleted);
        }

        public async Task<TicketValidation?> GetLastValidationAsync(Guid ticketId)
        {
            return await _context.TicketValidations
                .Where(v => v.TicketId == ticketId && !v.IsDeleted)
                .OrderByDescending(v => v.ValidationTime)
                .FirstOrDefaultAsync();
        }
    }
}