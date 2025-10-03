using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TransitAFC.Services.Ticket.Core.Models;

namespace TransitAFC.Services.Ticket.Infrastructure.Repositories
{
    public class TicketQRCodeRepository : ITicketQRCodeRepository
    {
        private readonly TicketDbContext _context;

        public TicketQRCodeRepository(TicketDbContext context)
        {
            _context = context;
        }

        public async Task<TicketQRCode> CreateAsync(TicketQRCode ticketQRCode)
        {
            _context.TicketQRCodes.Add(ticketQRCode);
            await _context.SaveChangesAsync();
            return ticketQRCode;
        }

        public async Task<IEnumerable<TicketQRCode>> GetByTicketIdAsync(Guid ticketId)
        {
            return await _context.TicketQRCodes
                .Where(h => h.TicketId == ticketId && !h.IsDeleted)
                .OrderByDescending(h => h.Version)
                .ToListAsync();
        }
    }
}
