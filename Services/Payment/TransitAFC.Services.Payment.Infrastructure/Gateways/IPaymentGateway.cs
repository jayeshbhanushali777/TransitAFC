using TransitAFC.Services.Payment.Core.DTOs;
using TransitAFC.Services.Payment.Core.Models;

namespace TransitAFC.Services.Payment.Infrastructure.Gateways
{
    public interface IPaymentGateway
    {
        PaymentGateway GatewayType { get; }
        Task<PaymentGatewayResponse> CreatePaymentAsync(PaymentGatewayRequest request);
        Task<PaymentGatewayResponse> VerifyPaymentAsync(string gatewayPaymentId);
        Task<RefundGatewayResponse> ProcessRefundAsync(RefundGatewayRequest request);
        Task<bool> ValidateWebhookSignatureAsync(string payload, string signature);
        Task<PaymentWebhookData> ProcessWebhookAsync(string payload);
        bool IsPaymentMethodSupported(PaymentMethod method);
        decimal CalculateGatewayFee(decimal amount, PaymentMethod method);
    }

    public class PaymentGatewayRequest
    {
        public string PaymentId { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string Currency { get; set; } = "INR";
        public PaymentMethod Method { get; set; }
        public string CustomerEmail { get; set; } = string.Empty;
        public string CustomerPhone { get; set; } = string.Empty;
        public string? CustomerName { get; set; }
        public string? Description { get; set; }
        public string? UpiId { get; set; }
        public string? CardToken { get; set; }
        public string? WalletType { get; set; }
        public string? BankCode { get; set; }
        public string? SuccessUrl { get; set; }
        public string? FailureUrl { get; set; }
        public Dictionary<string, object>? Metadata { get; set; }
    }

    public class PaymentGatewayResponse
    {
        public bool IsSuccess { get; set; }
        public string? GatewayPaymentId { get; set; }
        public string? GatewayOrderId { get; set; }
        public string? CheckoutUrl { get; set; }
        public string? PaymentToken { get; set; }
        public PaymentStatus Status { get; set; }
        public string? ErrorCode { get; set; }
        public string? ErrorMessage { get; set; }
        public decimal? GatewayFee { get; set; }
        public Dictionary<string, object>? AdditionalData { get; set; }
        public string? RawResponse { get; set; }
    }

    public class RefundGatewayRequest
    {
        public string GatewayPaymentId { get; set; } = string.Empty;
        public string RefundId { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string Currency { get; set; } = "INR";
        public string Reason { get; set; } = string.Empty;
        public string? Notes { get; set; }
    }

    public class RefundGatewayResponse
    {
        public bool IsSuccess { get; set; }
        public string? GatewayRefundId { get; set; }
        public RefundStatus Status { get; set; }
        public string? ErrorCode { get; set; }
        public string? ErrorMessage { get; set; }
        public int EstimatedDays { get; set; } = 7;
        public string? RawResponse { get; set; }
    }

    public class PaymentWebhookData
    {
        public string PaymentId { get; set; } = string.Empty;
        public string GatewayPaymentId { get; set; } = string.Empty;
        public PaymentStatus Status { get; set; }
        public string EventType { get; set; } = string.Empty;
        public decimal? Amount { get; set; }
        public string? ErrorCode { get; set; }
        public string? ErrorMessage { get; set; }
        public Dictionary<string, object>? AdditionalData { get; set; }
    }
}