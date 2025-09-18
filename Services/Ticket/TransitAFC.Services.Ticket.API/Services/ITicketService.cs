using TransitAFC.Services.Ticket.Core.DTOs;

namespace TransitAFC.Services.Ticket.API.Services
{
    public interface ITicketService
    {
        Task<TicketResponse> CreateTicketAsync(Guid userId, CreateTicketRequest request);
        Task<TicketResponse?> GetTicketAsync(Guid ticketId, Guid? userId = null);
        Task<TicketResponse?> GetTicketByNumberAsync(string ticketNumber, Guid? userId = null);
        Task<TicketResponse?> GetTicketByBookingIdAsync(Guid bookingId, Guid? userId = null);
        Task<List<TicketResponse>> GetUserTicketsAsync(Guid userId, int skip = 0, int take = 100);
        Task<List<TicketResponse>> SearchTicketsAsync(TicketSearchRequest request);
        Task<TicketResponse> ActivateTicketAsync(Guid ticketId, Guid userId);
        Task<TicketResponse> CancelTicketAsync(Guid ticketId, Guid userId, CancelTicketRequest request);
        Task<TicketValidationResult> ValidateTicketAsync(ValidateTicketRequest request);
        Task<TicketQRCodeResponse> RegenerateQRCodeAsync(Guid ticketId, Guid userId, RegenerateQRRequest request);
        Task<byte[]> GetQRCodeImageAsync(Guid ticketId, Guid userId);
        Task<TicketStatsResponse> GetStatsAsync(DateTime? fromDate = null, DateTime? toDate = null);
        Task<List<TicketResponse>> BulkOperationAsync(BulkTicketOperation operation);
        Task<TicketTransferResponse> ProcessTransferAsync(Guid ticketId, TicketTransferRequest request);
        Task ProcessExpiredTicketsAsync();
        Task<bool> CanUseTicketAsync(Guid ticketId);
        Task<int> GetRemainingUsageAsync(Guid ticketId);
    }
}