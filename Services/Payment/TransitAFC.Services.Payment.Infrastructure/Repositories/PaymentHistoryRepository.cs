using Microsoft.EntityFrameworkCore;
using TransitAFC.Services.Payment.Core.Models;

namespace TransitAFC.Services.Payment.Infrastructure.Repositories
{
    public class PaymentHistoryRepository : IPaymentHistoryRepository
    {
        private readonly PaymentDbContext _context;

        public PaymentHistoryRepository(PaymentDbContext context)
        {
            _context = context;
        }

        public async Task<PaymentHistory> CreateAsync(PaymentHistory history)
        {
            _context.PaymentHistory.Add(history);
            await _context.SaveChangesAsync();
            return history;
        }

        public async Task<IEnumerable<PaymentHistory>> GetByPaymentIdAsync(Guid paymentId)
        {
            return await _context.PaymentHistory
                .Where(h => h.PaymentId == paymentId)
                .OrderByDescending(h => h.ActionTime)
                .ToListAsync();
        }
    }
}