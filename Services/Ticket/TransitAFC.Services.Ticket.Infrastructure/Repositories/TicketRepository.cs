using Microsoft.EntityFrameworkCore;
using TransitAFC.Services.Ticket.Core.DTOs;
using TransitAFC.Services.Ticket.Core.Models;

namespace TransitAFC.Services.Ticket.Infrastructure.Repositories
{
    public class TicketRepository : ITicketRepository
    {
        private readonly TicketDbContext _context;

        public TicketRepository(TicketDbContext context)
        {
            _context = context;
        }

        public async Task<Core.Models.Ticket?> GetByIdAsync(Guid id)
        {
            return await _context.Tickets
                .Include(t => t.Validations)
                .Include(t => t.QRCodes)
                .Include(t => t.TicketHistory)
                .Include(t => t.Transfers)
                .FirstOrDefaultAsync(t => t.Id == id && !t.IsDeleted);
        }

        public async Task<Core.Models.Ticket?> GetByTicketNumberAsync(string ticketNumber)
        {
            return await _context.Tickets
                .Include(t => t.Validations)
                .Include(t => t.QRCodes)
                .Include(t => t.TicketHistory)
                .Include(t => t.Transfers)
                .FirstOrDefaultAsync(t => t.TicketNumber == ticketNumber && !t.IsDeleted);
        }

        public async Task<Core.Models.Ticket?> GetByBookingIdAsync(Guid bookingId)
        {
            return await _context.Tickets
                .Include(t => t.Validations)
                .Include(t => t.QRCodes)
                .Include(t => t.Transfers)
                .FirstOrDefaultAsync(t => t.BookingId == bookingId && !t.IsDeleted);
        }

        public async Task<Core.Models.Ticket?> GetByQRCodeDataAsync(string qrCodeData)
        {
            return await _context.Tickets
                .Include(t => t.Validations)
                .Include(t => t.QRCodes)
                .FirstOrDefaultAsync(t => t.QRCodeData == qrCodeData && !t.IsDeleted);
        }

        public async Task<IEnumerable<Core.Models.Ticket>> GetByUserIdAsync(Guid userId, int skip = 0, int take = 100)
        {
            return await _context.Tickets
                .Include(t => t.Validations)
                .Include(t => t.QRCodes)
                .Include(t => t.Transfers)
                .Where(t => t.UserId == userId && !t.IsDeleted)
                .OrderByDescending(t => t.CreatedAt)
                .Skip(skip)
                .Take(take)
                .ToListAsync();
        }

        public async Task<IEnumerable<Core.Models.Ticket>> SearchAsync(TicketSearchRequest request)
        {
            var query = _context.Tickets
                .Include(t => t.Validations)
                .Include(t => t.QRCodes)
                .Include(t => t.Transfers)
                .Where(t => !t.IsDeleted);

            if (request.UserId.HasValue)
                query = query.Where(t => t.UserId == request.UserId);

            if (!string.IsNullOrEmpty(request.TicketNumber))
                query = query.Where(t => t.TicketNumber.Contains(request.TicketNumber));

            if (request.BookingId.HasValue)
                query = query.Where(t => t.BookingId == request.BookingId);

            if (!string.IsNullOrEmpty(request.BookingNumber))
                query = query.Where(t => t.BookingNumber.Contains(request.BookingNumber));

            if (request.Status.HasValue)
                query = query.Where(t => t.Status == request.Status);

            if (request.Type.HasValue)
                query = query.Where(t => t.Type == request.Type);

            if (request.TransportMode.HasValue)
                query = query.Where(t => t.TransportMode == request.TransportMode);

            if (request.SourceStationId.HasValue)
                query = query.Where(t => t.SourceStationId == request.SourceStationId);

            if (request.DestinationStationId.HasValue)
                query = query.Where(t => t.DestinationStationId == request.DestinationStationId);

            if (request.ValidFromDate.HasValue)
                query = query.Where(t => t.ValidFrom >= request.ValidFromDate);

            if (request.ValidToDate.HasValue)
                query = query.Where(t => t.ValidUntil <= request.ValidToDate);

            if (request.CreatedFromDate.HasValue)
                query = query.Where(t => t.CreatedAt >= request.CreatedFromDate);

            if (request.CreatedToDate.HasValue)
                query = query.Where(t => t.CreatedAt <= request.CreatedToDate);

            if (!string.IsNullOrEmpty(request.PassengerName))
                query = query.Where(t => t.PassengerName.Contains(request.PassengerName));

            if (!string.IsNullOrEmpty(request.PassengerPhone))
                query = query.Where(t => t.PassengerPhone != null && t.PassengerPhone.Contains(request.PassengerPhone));

            if (!string.IsNullOrEmpty(request.PassengerEmail))
                query = query.Where(t => t.PassengerEmail != null && t.PassengerEmail.Contains(request.PassengerEmail));

            return await query
                .OrderByDescending(t => t.CreatedAt)
                .Skip(request.Skip)
                .Take(request.Take)
                .ToListAsync();
        }

        public async Task<Core.Models.Ticket> CreateAsync(Core.Models.Ticket ticket)
        {
            try
            {
                _context.Tickets.Add(ticket);
                await _context.SaveChangesAsync();
                
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return ticket;
        }

        public async Task<Core.Models.Ticket> UpdateAsync(Core.Models.Ticket ticket)
        {
            ticket.UpdatedAt = DateTime.UtcNow;
            _context.Tickets.Update(ticket);
            await _context.SaveChangesAsync();
            return ticket;
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var ticket = await GetByIdAsync(id);
            if (ticket == null) return false;

            ticket.IsDeleted = true;
            ticket.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ExistsAsync(string ticketNumber)
        {
            return await _context.Tickets
                .AnyAsync(t => t.TicketNumber == ticketNumber && !t.IsDeleted);
        }

        public async Task<string> GenerateTicketNumberAsync()
        {
            string prefix = "TKT";
            string datePart = DateTime.UtcNow.ToString("yyyyMMdd");

            var lastTicket = await _context.Tickets
                .Where(t => t.TicketNumber.StartsWith(prefix + datePart))
                .OrderByDescending(t => t.TicketNumber)
                .FirstOrDefaultAsync();

            int sequence = 1;
            if (lastTicket != null)
            {
                var lastSequence = lastTicket.TicketNumber.Substring(prefix.Length + datePart.Length);
                if (int.TryParse(lastSequence, out int lastSeq))
                {
                    sequence = lastSeq + 1;
                }
            }

            return $"{prefix}{datePart}{sequence:D6}";
        }

        public async Task<TicketStatsResponse> GetStatsAsync(DateTime? fromDate = null, DateTime? toDate = null)
        {
            var query = _context.Tickets.Where(t => !t.IsDeleted);

            if (fromDate.HasValue)
                query = query.Where(t => t.CreatedAt >= fromDate);

            if (toDate.HasValue)
                query = query.Where(t => t.CreatedAt <= toDate);

            var tickets = await query.ToListAsync();
            var validations = await _context.TicketValidations
                .Where(v => !v.IsDeleted &&
                           (fromDate == null || v.ValidationTime >= fromDate) &&
                           (toDate == null || v.ValidationTime <= toDate))
                .ToListAsync();

            return new TicketStatsResponse
            {
                TotalTickets = tickets.Count,
                ActiveTickets = tickets.Count(t => t.Status == TicketStatus.Active),
                UsedTickets = tickets.Count(t => t.Status == TicketStatus.Used),
                ExpiredTickets = tickets.Count(t => t.Status == TicketStatus.Expired),
                CancelledTickets = tickets.Count(t => t.Status == TicketStatus.Cancelled),
                TotalRevenue = tickets.Sum(t => t.FinalPrice),
                AverageTicketPrice = tickets.Count > 0 ? tickets.Average(t => t.FinalPrice) : 0,
                TicketsByType = tickets.GroupBy(t => t.Type.ToString()).ToDictionary(g => g.Key, g => g.Count()),
                TicketsByStatus = tickets.GroupBy(t => t.Status.ToString()).ToDictionary(g => g.Key, g => g.Count()),
                TicketsByTransportMode = tickets.GroupBy(t => t.TransportMode.ToString()).ToDictionary(g => g.Key, g => g.Count()),
                RevenueByDate = tickets
                    .GroupBy(t => t.CreatedAt.Date.ToString("yyyy-MM-dd"))
                    .ToDictionary(g => g.Key, g => g.Sum(t => t.FinalPrice)),
                UsageByStation = validations
                    .GroupBy(v => v.StationName)
                    .ToDictionary(g => g.Key, g => g.Count()),
                UsageRate = tickets.Count > 0 ? (decimal)tickets.Count(t => t.UsageCount > 0) / tickets.Count * 100 : 0,
                TotalValidations = validations.Count,
                SuccessfulValidations = validations.Count(v => v.ValidationResult == ValidationResult.Valid),
                FailedValidations = validations.Count(v => v.ValidationResult != ValidationResult.Valid),
                ValidationSuccessRate = validations.Count > 0 ? (decimal)validations.Count(v => v.ValidationResult == ValidationResult.Valid) / validations.Count * 100 : 0
            };
        }

        public async Task<IEnumerable<Core.Models.Ticket>> GetExpiredTicketsAsync()
        {
            return await _context.Tickets
                .Where(t => t.Status == TicketStatus.Active &&
                           t.ValidUntil < DateTime.UtcNow &&
                           !t.IsDeleted)
                .ToListAsync();
        }

        public async Task<IEnumerable<Core.Models.Ticket>> GetActiveTicketsAsync()
        {
            return await _context.Tickets
                .Where(t => t.Status == TicketStatus.Active && !t.IsDeleted)
                .OrderBy(t => t.ValidUntil)
                .ToListAsync();
        }

        public async Task<IEnumerable<Core.Models.Ticket>> GetTicketsByStationAsync(Guid stationId, DateTime? date = null)
        {
            var query = _context.Tickets
                .Where(t => (t.SourceStationId == stationId || t.DestinationStationId == stationId) &&
                           !t.IsDeleted);

            if (date.HasValue)
            {
                var startDate = date.Value.Date;
                var endDate = startDate.AddDays(1);
                query = query.Where(t => t.CreatedAt >= startDate && t.CreatedAt < endDate);
            }

            return await query.OrderByDescending(t => t.CreatedAt).ToListAsync();
        }

        public async Task<int> GetUsageCountAsync(Guid ticketId)
        {
            return await _context.TicketValidations
                .Where(v => v.TicketId == ticketId &&
                           v.ValidationResult == ValidationResult.Valid &&
                           !v.IsDeleted)
                .CountAsync();
        }

        public async Task<bool> CanUseTicketAsync(Guid ticketId)
        {
            var ticket = await _context.Tickets
                .FirstOrDefaultAsync(t => t.Id == ticketId && !t.IsDeleted);

            if (ticket == null) return false;

            return ticket.Status == TicketStatus.Active &&
                   ticket.ValidFrom <= DateTime.UtcNow &&
                   ticket.ValidUntil > DateTime.UtcNow &&
                   ticket.UsageCount < ticket.MaxUsageCount;
        }
    }
}