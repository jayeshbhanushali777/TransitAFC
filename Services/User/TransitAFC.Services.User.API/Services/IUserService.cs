using TransitAFC.Services.User.Core.DTOs;

namespace TransitAFC.Services.User.API.Services
{
    public interface IUserService
    {
        Task<UserResponse> RegisterAsync(RegisterUserRequest request);
        Task<LoginResponse> LoginAsync(LoginRequest request);
        Task<UserResponse?> GetProfileAsync(Guid userId);
        Task<UserResponse> UpdateProfileAsync(Guid userId, UpdateUserRequest request);
        Task<bool> DeleteAccountAsync(Guid userId);
        Task<bool> VerifyEmailAsync(Guid userId, string verificationCode);
        Task<bool> VerifyPhoneAsync(Guid userId, string verificationCode);
    }
}