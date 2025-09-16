using TransitAFC.Services.Payment.Core.DTOs;
using TransitAFC.Services.Payment.Core.Models;

namespace TransitAFC.Services.Payment.API.Services
{
    public interface IPaymentService
    {
        Task<PaymentResponse> CreatePaymentAsync(Guid userId, CreatePaymentRequest request);
        Task<PaymentResponse?> GetPaymentAsync(Guid paymentId, Guid? userId = null);
        Task<PaymentResponse?> GetPaymentByIdAsync(string paymentId, Guid? userId = null);
        Task<PaymentResponse?> GetPaymentByBookingIdAsync(Guid bookingId, Guid? userId = null);
        Task<List<PaymentResponse>> GetUserPaymentsAsync(Guid userId, int skip = 0, int take = 100);
        Task<List<PaymentResponse>> SearchPaymentsAsync(PaymentSearchRequest request);
        Task<PaymentResponse> ProcessPaymentAsync(ProcessPaymentRequest request);
        Task<PaymentResponse> VerifyPaymentAsync(PaymentVerificationRequest request);
        Task<PaymentRefundResponse> ProcessRefundAsync(Guid paymentId, Guid userId, RefundPaymentRequest request);
        Task<PaymentStatsResponse> GetStatsAsync(DateTime? fromDate = null, DateTime? toDate = null);
        Task<PaymentResponse> HandleWebhookAsync(PaymentWebhookRequest request);
        Task ProcessExpiredPaymentsAsync();
        Task<List<PaymentMethod>> GetSupportedPaymentMethodsAsync();
        Task<decimal> CalculateGatewayFeeAsync(decimal amount, PaymentMethod method, PaymentGateway? gateway = null);
    }
}