using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TransitAFC.Services.Payment.Core.Models;

namespace TransitAFC.Services.Payment.Infrastructure.Repositories
{
    public class PaymentTransactionRepository : IPaymentTransactionRepository
    {
        private readonly PaymentDbContext _context;

        public PaymentTransactionRepository(PaymentDbContext context)
        {
            _context = context;
        }

        public async Task<PaymentTransaction> CreateAsync(PaymentTransaction transaction)
        {
            _context.PaymentTransactions.Add(transaction);
            await _context.SaveChangesAsync();
            return transaction;
        }

        public async Task<IEnumerable<PaymentTransaction>> GetByPaymentIdAsync(Guid paymentId)
        {
            return await _context.PaymentTransactions
                .Where(h => h.PaymentId == paymentId)
                .OrderByDescending(h => h.ProcessedAt)
                .ToListAsync();
        }
    }
}
