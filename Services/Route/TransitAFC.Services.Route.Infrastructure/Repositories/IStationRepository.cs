using TransitAFC.Services.Route.Core.Models;

namespace TransitAFC.Services.Route.Infrastructure.Repositories
{
    public interface IStationRepository
    {
        Task<Station?> GetByIdAsync(Guid id);
        Task<Station?> GetByCodeAsync(string code);
        Task<IEnumerable<Station>> GetAllAsync(int skip = 0, int take = 100);
        Task<IEnumerable<Station>> GetByTransportModeAsync(Guid transportModeId);
        Task<IEnumerable<Station>> GetNearbyStationsAsync(double latitude, double longitude, double radiusKm);
        Task<IEnumerable<Station>> SearchStationsAsync(string searchTerm);
        Task<Station> CreateAsync(Station station);
        Task<Station> UpdateAsync(Station station);
        Task<bool> DeleteAsync(Guid id);
        Task<bool> ExistsAsync(string code);
        Task<IEnumerable<Station>> GetStationsByNameAsync(string name);
    }
}