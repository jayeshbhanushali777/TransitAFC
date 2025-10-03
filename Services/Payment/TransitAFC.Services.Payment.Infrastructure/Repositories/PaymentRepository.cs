using Microsoft.EntityFrameworkCore;
using TransitAFC.Services.Payment.Core.DTOs;
using TransitAFC.Services.Payment.Core.Models;
using TransitAFC.Services.Payment.Infrastructure.Repositories;

namespace TransitAFC.Services.Payment.Infrastructure.Repositories
{
    public class PaymentRepository : IPaymentRepository
    {
        private readonly PaymentDbContext _context;

        public PaymentRepository(PaymentDbContext context)
        {
            _context = context;
        }

        public async Task<Core.Models.Payment?> GetByIdAsync(Guid id)
        {
            return await _context.Payments
                .Include(p => p.Transactions)
                .Include(p => p.Refunds)
                .Include(p => p.PaymentHistory)
                .FirstOrDefaultAsync(p => p.Id == id && !p.IsDeleted);
        }

        public async Task<Core.Models.Payment?> GetByPaymentIdAsync(string paymentId)
        {
            return await _context.Payments
                .Include(p => p.Transactions)
                .Include(p => p.Refunds)
                .Include(p => p.PaymentHistory)
                .FirstOrDefaultAsync(p => p.PaymentId == paymentId && !p.IsDeleted);
        }

        public async Task<Core.Models.Payment?> GetByBookingIdAsync(Guid bookingId)
        {
            return await _context.Payments
                .Include(p => p.Transactions)
                .Include(p => p.Refunds)
                .FirstOrDefaultAsync(p => p.BookingId == bookingId && !p.IsDeleted);
        }

        public async Task<Core.Models.Payment?> GetByGatewayPaymentIdAsync(string gatewayPaymentId)
        {
            return await _context.Payments
                .Include(p => p.Transactions)
                .Include(p => p.Refunds)
                .FirstOrDefaultAsync(p => p.GatewayPaymentId == gatewayPaymentId && !p.IsDeleted);
        }

        public async Task<IEnumerable<Core.Models.Payment>> GetByUserIdAsync(Guid userId, int skip = 0, int take = 100)
        {
            return await _context.Payments
                .Include(p => p.Transactions)
                .Include(p => p.Refunds)
                .Where(p => p.UserId == userId && !p.IsDeleted)
                .OrderByDescending(p => p.CreatedAt)
                .Skip(skip)
                .Take(take)
                .ToListAsync();
        }

        public async Task<IEnumerable<Core.Models.Payment>> SearchAsync(PaymentSearchRequest request)
        {
            var query = _context.Payments
                .Include(p => p.Transactions)
                .Include(p => p.Refunds)
                .Where(p => !p.IsDeleted);

            if (request.UserId.HasValue)
                query = query.Where(p => p.UserId == request.UserId);

            if (request.BookingId.HasValue)
                query = query.Where(p => p.BookingId == request.BookingId);

            if (!string.IsNullOrEmpty(request.PaymentId))
                query = query.Where(p => p.PaymentId.Contains(request.PaymentId));

            if (!string.IsNullOrEmpty(request.BookingNumber))
                query = query.Where(p => p.BookingNumber.Contains(request.BookingNumber));

            if (request.Status.HasValue)
                query = query.Where(p => p.Status == request.Status);

            if (request.Method.HasValue)
                query = query.Where(p => p.Method == request.Method);

            if (request.Gateway.HasValue)
                query = query.Where(p => p.Gateway == request.Gateway);

            if (request.FromDate.HasValue)
                query = query.Where(p => p.CreatedAt >= request.FromDate);

            if (request.ToDate.HasValue)
                query = query.Where(p => p.CreatedAt <= request.ToDate);

            if (request.MinAmount.HasValue)
                query = query.Where(p => p.TotalAmount >= request.MinAmount);

            if (request.MaxAmount.HasValue)
                query = query.Where(p => p.TotalAmount <= request.MaxAmount);

            if (!string.IsNullOrEmpty(request.CustomerEmail))
                query = query.Where(p => p.CustomerEmail.Contains(request.CustomerEmail));

            if (!string.IsNullOrEmpty(request.CustomerPhone))
                query = query.Where(p => p.CustomerPhone.Contains(request.CustomerPhone));

            if (!string.IsNullOrEmpty(request.TransactionId))
                query = query.Where(p => p.TransactionId != null && p.TransactionId.Contains(request.TransactionId));

            return await query
                .OrderByDescending(p => p.CreatedAt)
                .Skip(request.Skip)
                .Take(request.Take)
                .ToListAsync();
        }

        public async Task<Core.Models.Payment> CreateAsync(Core.Models.Payment payment)
        {
            _context.Payments.Add(payment);
            await _context.SaveChangesAsync();
            return payment;
        }

        public async Task<Core.Models.Payment> UpdateAsync(Core.Models.Payment payment)
        {
            payment.UpdatedAt = DateTime.UtcNow;
            _context.Payments.Update(payment);
                await _context.SaveChangesAsync();
            return payment;
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var payment = await GetByIdAsync(id);
            if (payment == null) return false;

            payment.IsDeleted = true;
            payment.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ExistsAsync(string paymentId)
        {
            return await _context.Payments
                .AnyAsync(p => p.PaymentId == paymentId && !p.IsDeleted);
        }

        public async Task<string> GeneratePaymentIdAsync()
        {
            string prefix = "PAY";
            string datePart = DateTime.UtcNow.ToString("yyyyMMdd");

            var lastPayment = await _context.Payments
                .Where(p => p.PaymentId.StartsWith(prefix + datePart))
                .OrderByDescending(p => p.PaymentId)
                .FirstOrDefaultAsync();

            int sequence = 1;
            if (lastPayment != null)
            {
                var lastSequence = lastPayment.PaymentId.Substring(prefix.Length + datePart.Length);
                if (int.TryParse(lastSequence, out int lastSeq))
                {
                    sequence = lastSeq + 1;
                }
            }

            return $"{prefix}{datePart}{sequence:D6}";
        }

        public async Task<PaymentStatsResponse> GetStatsAsync(DateTime? fromDate = null, DateTime? toDate = null)
        {
            var query = _context.Payments.Where(p => !p.IsDeleted);

            if (fromDate.HasValue)
                query = query.Where(p => p.CreatedAt >= fromDate);

            if (toDate.HasValue)
                query = query.Where(p => p.CreatedAt <= toDate);

            var payments = await query.ToListAsync();

            var completedPayments = payments.Where(p => p.Status == PaymentStatus.Completed).ToList();
            var totalGatewayFees = completedPayments.Sum(p => p.GatewayFee ?? 0);

            return new PaymentStatsResponse
            {
                TotalPayments = payments.Count,
                PendingPayments = payments.Count(p => p.Status == PaymentStatus.Pending),
                CompletedPayments = payments.Count(p => p.Status == PaymentStatus.Completed),
                FailedPayments = payments.Count(p => p.Status == PaymentStatus.Failed),
                RefundedPayments = payments.Count(p => p.Status == PaymentStatus.Refunded || p.Status == PaymentStatus.PartiallyRefunded),
                TotalAmount = payments.Sum(p => p.TotalAmount),
                CompletedAmount = completedPayments.Sum(p => p.TotalAmount),
                RefundedAmount = payments.Sum(p => p.RefundedAmount),
                PendingAmount = payments.Where(p => p.Status == PaymentStatus.Pending).Sum(p => p.TotalAmount),
                GatewayFees = totalGatewayFees,
                NetRevenue = completedPayments.Sum(p => p.TotalAmount) - totalGatewayFees,
                PaymentsByMethod = payments.GroupBy(p => p.Method.ToString()).ToDictionary(g => g.Key, g => g.Count()),
                PaymentsByGateway = payments.GroupBy(p => p.Gateway.ToString()).ToDictionary(g => g.Key, g => g.Count()),
                RevenueByDate = completedPayments
                    .GroupBy(p => p.CreatedAt.Date.ToString("yyyy-MM-dd"))
                    .ToDictionary(g => g.Key, g => g.Sum(p => p.TotalAmount)),
                PaymentsByStatus = payments.GroupBy(p => p.Status.ToString()).ToDictionary(g => g.Key, g => g.Count()),
                SuccessRate = payments.Count > 0 ? (decimal)completedPayments.Count / payments.Count * 100 : 0,
                AverageTransactionAmount = payments.Count > 0 ? payments.Average(p => p.TotalAmount) : 0
            };
        }

        public async Task<IEnumerable<Core.Models.Payment>> GetExpiredPaymentsAsync()
        {
            return await _context.Payments
                .Where(p => p.Status == PaymentStatus.Pending &&
                           p.ExpiresAt.HasValue &&
                           p.ExpiresAt < DateTime.UtcNow &&
                           !p.IsDeleted)
                .ToListAsync();
        }

        public async Task<IEnumerable<Core.Models.Payment>> GetPendingPaymentsAsync()
        {
            return await _context.Payments
                .Where(p => p.Status == PaymentStatus.Pending && !p.IsDeleted)
                .OrderBy(p => p.CreatedAt)
                .ToListAsync();
        }
    }
}