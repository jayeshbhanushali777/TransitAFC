using TransitAFC.Services.Route.Core.Models;

namespace TransitAFC.Services.Route.Infrastructure.Repositories
{
    public interface IRouteRepository
    {
        Task<Core.Models.Route?> GetByIdAsync(Guid id);
        Task<Core.Models.Route?> GetByCodeAsync(string code);
        Task<IEnumerable<Core.Models.Route>> GetAllAsync(int skip = 0, int take = 100);
        Task<IEnumerable<Core.Models.Route>> GetByTransportModeAsync(Guid transportModeId);
        Task<IEnumerable<Core.Models.Route>> GetRoutesBetweenStationsAsync(Guid startStationId, Guid endStationId);
        Task<IEnumerable<Core.Models.Route>> SearchRoutesAsync(string searchTerm);
        Task<Core.Models.Route> CreateAsync(Core.Models.Route route);
        Task<Core.Models.Route> UpdateAsync(Core.Models.Route route);
        Task<bool> DeleteAsync(Guid id);
        Task<bool> ExistsAsync(string code);
        Task<IEnumerable<Core.Models.Route>> GetActiveRoutesAsync();
        Task<IEnumerable<RouteStation>> GetRouteStationsAsync(Guid routeId);
    }
}