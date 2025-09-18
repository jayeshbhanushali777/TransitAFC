using AutoMapper;
using TransitAFC.Services.Ticket.Core.DTOs;
using TransitAFC.Services.Ticket.Core.Models;

namespace TransitAFC.Services.Ticket.API.Mapping
{
    public class TicketMappingProfile : Profile
    {
        public TicketMappingProfile()
        {
            // Ticket mappings
            CreateMap<Core.Models.Ticket, TicketResponse>();

            CreateMap<CreateTicketRequest, Core.Models.Ticket>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.TicketNumber, opt => opt.Ignore())
                .ForMember(dest => dest.UserId, opt => opt.Ignore())
                .ForMember(dest => dest.Status, opt => opt.Ignore())
                .ForMember(dest => dest.QRCodeData, opt => opt.Ignore())
                .ForMember(dest => dest.QRCodeHash, opt => opt.Ignore())
                .ForMember(dest => dest.QRCodeImage, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
                .ForMember(dest => dest.IsActive, opt => opt.Ignore())
                .ForMember(dest => dest.Validations, opt => opt.Ignore())
                .ForMember(dest => dest.QRCodes, opt => opt.Ignore())
                .ForMember(dest => dest.TicketHistory, opt => opt.Ignore())
                .ForMember(dest => dest.Transfers, opt => opt.Ignore());

            // Validation mappings
            CreateMap<TicketValidation, TicketValidationResponse>();

            // QR Code mappings
            CreateMap<TicketQRCode, TicketQRCodeResponse>();

            // Transfer mappings
            CreateMap<TicketTransfer, TicketTransferResponse>();

            // History mappings
            CreateMap<TicketHistory, TicketHistoryResponse>();
        }
    }

    public class TicketHistoryResponse
    {
        public Guid Id { get; set; }
        public TicketStatus FromStatus { get; set; }
        public TicketStatus ToStatus { get; set; }
        public string Action { get; set; } = string.Empty;
        public string? Reason { get; set; }
        public string? Notes { get; set; }
        public Guid ActionBy { get; set; }
        public string ActionByType { get; set; } = string.Empty;
        public string? ActionByName { get; set; }
        public DateTime ActionTime { get; set; }
    }
}