using TransitAFC.Services.Payment.Core.DTOs;
using TransitAFC.Services.Payment.Core.Models;

namespace TransitAFC.Services.Payment.Infrastructure.Repositories
{
    public interface IPaymentRepository
    {
        Task<Core.Models.Payment?> GetByIdAsync(Guid id);
        Task<Core.Models.Payment?> GetByPaymentIdAsync(string paymentId);
        Task<Core.Models.Payment?> GetByBookingIdAsync(Guid bookingId);
        Task<IEnumerable<Core.Models.Payment>> GetByUserIdAsync(Guid userId, int skip = 0, int take = 100);
        Task<IEnumerable<Core.Models.Payment>> SearchAsync(PaymentSearchRequest request);
        Task<Core.Models.Payment> CreateAsync(Core.Models.Payment payment);
        Task<Core.Models.Payment> UpdateAsync(Core.Models.Payment payment);
        Task<bool> DeleteAsync(Guid id);
        Task<bool> ExistsAsync(string paymentId);
        Task<string> GeneratePaymentIdAsync();
        Task<PaymentStatsResponse> GetStatsAsync(DateTime? fromDate = null, DateTime? toDate = null);
        Task<IEnumerable<Core.Models.Payment>> GetExpiredPaymentsAsync();
        Task<IEnumerable<Core.Models.Payment>> GetPendingPaymentsAsync();
        Task<Core.Models.Payment?> GetByGatewayPaymentIdAsync(string gatewayPaymentId);
    }
}