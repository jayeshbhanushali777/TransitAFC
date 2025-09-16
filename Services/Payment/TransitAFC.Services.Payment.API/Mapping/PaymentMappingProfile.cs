using AutoMapper;
using TransitAFC.Services.Payment.Core.DTOs;
using TransitAFC.Services.Payment.Core.Models;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace TransitAFC.Services.Payment.API.Mapping
{
    public class PaymentMappingProfile : Profile
    {
        public PaymentMappingProfile()
        {
            // Payment mappings
            CreateMap<Core.Models.Payment, PaymentResponse>()
                .ForMember(dest => dest.GatewayInfo, opt => opt.Ignore());

            CreateMap<CreatePaymentRequest, Core.Models.Payment>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.PaymentId, opt => opt.Ignore())
                .ForMember(dest => dest.UserId, opt => opt.Ignore())
                .ForMember(dest => dest.Status, opt => opt.Ignore())
                .ForMember(dest => dest.Gateway, opt => opt.Ignore())
                .ForMember(dest => dest.Mode, opt => opt.Ignore())
                .ForMember(dest => dest.TaxAmount, opt => opt.Ignore())
                .ForMember(dest => dest.ServiceFee, opt => opt.Ignore())
                .ForMember(dest => dest.GatewayFee, opt => opt.Ignore())
                .ForMember(dest => dest.TotalAmount, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
                .ForMember(dest => dest.IsActive, opt => opt.Ignore())
                .ForMember(dest => dest.Transactions, opt => opt.Ignore())
                .ForMember(dest => dest.Refunds, opt => opt.Ignore())
                .ForMember(dest => dest.PaymentHistory, opt => opt.Ignore());

            // Transaction mappings
            CreateMap<PaymentTransaction, PaymentTransactionResponse>();

            // Refund mappings
            CreateMap<PaymentRefund, PaymentRefundResponse>();
        }
    }
}