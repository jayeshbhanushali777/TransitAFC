namespace TransitAFC.Services.Payment.API.Services
{
    public class BookingInfo
    {
        public Guid Id { get; set; }
        public string BookingNumber { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public Guid UserId { get; set; }
    }

    public interface IBookingService
    {
        Task<BookingInfo?> GetBookingAsync(Guid bookingId, Guid userId);
        Task<bool> ConfirmBookingAsync(Guid bookingId, Guid userId, object confirmationData);
    }
}