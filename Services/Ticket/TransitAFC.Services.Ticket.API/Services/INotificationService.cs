namespace TransitAFC.Services.Ticket.API.Services
{
    public interface INotificationService
    {
        Task SendTicketGeneratedNotificationAsync(Guid userId, string ticketNumber);
        Task SendTicketActivatedNotificationAsync(Guid userId, string ticketNumber);
        Task SendTicketCancelledNotificationAsync(Guid userId, string ticketNumber, string reason);
        Task SendTicketExpiredNotificationAsync(Guid userId, string ticketNumber);
    }
}
