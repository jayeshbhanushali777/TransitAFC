using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TransitAFC.Services.Ticket.Core.Models;

namespace TransitAFC.Services.Ticket.Infrastructure.Repositories
{
    public interface ITicketQRCodeRepository
    {
        Task<TicketQRCode> CreateAsync(TicketQRCode ticketQRCode);
        Task<IEnumerable<TicketQRCode>> GetByTicketIdAsync(Guid ticketId);
    }
}
