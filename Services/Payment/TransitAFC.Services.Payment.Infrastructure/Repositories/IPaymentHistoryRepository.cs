using TransitAFC.Services.Payment.Core.Models;

namespace TransitAFC.Services.Payment.Infrastructure.Repositories
{
    public interface IPaymentHistoryRepository
    {
        Task<PaymentHistory> CreateAsync(PaymentHistory history);
        Task<IEnumerable<PaymentHistory>> GetByPaymentIdAsync(Guid paymentId);
    }
}