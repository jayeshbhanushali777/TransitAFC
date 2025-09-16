using TransitAFC.Services.User.Core.Models;

namespace TransitAFC.Services.User.Infrastructure.Repositories
{
    public interface IUserRepository
    {
        Task<Core.Models.User?> GetByIdAsync(Guid id);
        Task<Core.Models.User?> GetByEmailAsync(string email);
        Task<Core.Models.User?> GetByPhoneAsync(string phoneNumber);
        Task<Core.Models.User> CreateAsync(Core.Models.User user);
        Task<Core.Models.User> UpdateAsync(Core.Models.User user);
        Task<bool> DeleteAsync(Guid id);
        Task<bool> ExistsAsync(string email);
        Task<IEnumerable<Core.Models.User>> GetAllAsync(int skip = 0, int take = 100);
    }
}