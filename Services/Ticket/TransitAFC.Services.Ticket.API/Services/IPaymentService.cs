namespace TransitAFC.Services.Ticket.API.Services
{
    public interface IPaymentService
    {
        Task<PaymentInfo?> GetPaymentByBookingIdAsync(Guid bookingId, Guid userId);
        Task<bool> ProcessRefundAsync(Guid paymentId, Guid userId, object refundRequest);
    }

}
