namespace TransitAFC.Services.Payment.API.Services
{
    public interface INotificationService
    {
        Task SendPaymentSuccessNotificationAsync(Guid userId, string paymentId);
        Task SendPaymentFailureNotificationAsync(Guid userId, string paymentId, string? reason);
        Task SendRefundInitiatedNotificationAsync(Guid userId, string refundId, decimal amount);
        Task SendRefundCompletedNotificationAsync(Guid userId, string refundId, decimal amount);
    }
}