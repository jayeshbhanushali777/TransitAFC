using AutoMapper;
using TransitAFC.Services.User.Core.DTOs;

namespace TransitAFC.Services.User.API.Mapping
{
    public class UserMappingProfile : Profile
    {
        public UserMappingProfile()
        {
            CreateMap<Core.Models.User, UserResponse>()
                .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => $"{src.FirstName} {src.LastName}"))
                .ForMember(dest => dest.IsEmailVerified, opt => opt.MapFrom(src => src.EmailVerifiedAt.HasValue))
                .ForMember(dest => dest.IsPhoneVerified, opt => opt.MapFrom(src => src.PhoneVerifiedAt.HasValue));

            CreateMap<RegisterUserRequest, Core.Models.User>()
                .ForMember(dest => dest.PasswordHash, opt => opt.Ignore())
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
                .ForMember(dest => dest.IsActive, opt => opt.Ignore());
        }
    }
}