using AutoMapper;
using BCrypt.Net;
using TransitAFC.Services.User.API.Services;
using TransitAFC.Services.User.Core.DTOs;
using TransitAFC.Services.User.Infrastructure.Repositories;
using TransitAFC.Shared.Security;

namespace TransitAFC.Services.User.API.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly IJwtService _jwtService;
        private readonly IMapper _mapper;

        public UserService(
            IUserRepository userRepository,
            IJwtService jwtService,
            IMapper mapper)
        {
            _userRepository = userRepository;
            _jwtService = jwtService;
            _mapper = mapper;
        }

        public async Task<UserResponse> RegisterAsync(RegisterUserRequest request)
        {
            // Check if user already exists
            if (await _userRepository.ExistsAsync(request.Email))
            {
                throw new InvalidOperationException("User with this email already exists");
            }

            // Create new user
            var user = new Core.Models.User
            {
                Email = request.Email.ToLower(),
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
                FirstName = request.FirstName,
                LastName = request.LastName,
                PhoneNumber = request.PhoneNumber,
                //DateOfBirth = request.DateOfBirth,
                Gender = request.Gender,
                City = request.City,
                State = request.State,
                PinCode = request.PinCode,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            var createdUser = await _userRepository.CreateAsync(user);
            return _mapper.Map<UserResponse>(createdUser);
        }

        public async Task<LoginResponse> LoginAsync(LoginRequest request)
        {
            var user = await _userRepository.GetByEmailAsync(request.Email);

            if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            {
                throw new UnauthorizedAccessException("Invalid email or password");
            }

            // Update last login
            user.LastLoginAt = DateTime.UtcNow;
            await _userRepository.UpdateAsync(user);

            // Replace this line:
            // var token = _jwtService.GenerateToken(user.Id, user.Email);

            // With the following line:
            var token = _jwtService.GenerateToken(user.Id, user.Email, user.FirstName, user.LastName, null);

            return new LoginResponse
            {
                Token = token,
                User = _mapper.Map<UserResponse>(user),
                ExpiresAt = DateTime.UtcNow.AddHours(24)
            };
        }

        public async Task<UserResponse?> GetProfileAsync(Guid userId)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            return user != null ? _mapper.Map<UserResponse>(user) : null;
        }

        public async Task<UserResponse> UpdateProfileAsync(Guid userId, UpdateUserRequest request)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
            {
                throw new InvalidOperationException("User not found");
            }

            // Update only provided fields
            if (!string.IsNullOrEmpty(request.FirstName))
                user.FirstName = request.FirstName;

            if (!string.IsNullOrEmpty(request.LastName))
                user.LastName = request.LastName;

            if (!string.IsNullOrEmpty(request.PhoneNumber))
                user.PhoneNumber = request.PhoneNumber;

            if (request.DateOfBirth.HasValue)
                user.DateOfBirth = request.DateOfBirth;

            if (!string.IsNullOrEmpty(request.Gender))
                user.Gender = request.Gender;

            if (!string.IsNullOrEmpty(request.City))
                user.City = request.City;

            if (!string.IsNullOrEmpty(request.State))
                user.State = request.State;

            if (!string.IsNullOrEmpty(request.PinCode))
                user.PinCode = request.PinCode;

            var updatedUser = await _userRepository.UpdateAsync(user);
            return _mapper.Map<UserResponse>(updatedUser);
        }

        public async Task<bool> DeleteAccountAsync(Guid userId)
        {
            return await _userRepository.DeleteAsync(userId);
        }

        public async Task<bool> VerifyEmailAsync(Guid userId, string verificationCode)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null) return false;

            // In a real implementation, you would verify the code
            user.EmailVerifiedAt = DateTime.UtcNow;
            await _userRepository.UpdateAsync(user);
            return true;
        }

        public async Task<bool> VerifyPhoneAsync(Guid userId, string verificationCode)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null) return false;

            // In a real implementation, you would verify the code
            user.PhoneVerifiedAt = DateTime.UtcNow;
            await _userRepository.UpdateAsync(user);
            return true;
        }
    }
}