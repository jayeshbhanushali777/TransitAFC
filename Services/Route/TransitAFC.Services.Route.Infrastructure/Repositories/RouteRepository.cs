using Microsoft.EntityFrameworkCore;
using TransitAFC.Services.Route.Core.Models;
using TransitAFC.Services.Route.Infrastructure.Repositories;

namespace TransitAFC.Services.Route.Infrastructure.Repositories
{
    public class RouteRepository : IRouteRepository
    {
        private readonly RouteDbContext _context;

        public RouteRepository(RouteDbContext context)
        {
            _context = context;
        }

        public async Task<Core.Models.Route?> GetByIdAsync(Guid id)
        {
            return await _context.Routes
                .Include(r => r.TransportMode)
                .Include(r => r.StartStation)
                .Include(r => r.EndStation)
                .Include(r => r.RouteStations)
                    .ThenInclude(rs => rs.Station)
                .FirstOrDefaultAsync(r => r.Id == id && !r.IsDeleted);
        }

        public async Task<Core.Models.Route?> GetByCodeAsync(string code)
        {
            return await _context.Routes
                .Include(r => r.TransportMode)
                .Include(r => r.StartStation)
                .Include(r => r.EndStation)
                .Include(r => r.RouteStations)
                    .ThenInclude(rs => rs.Station)
                .FirstOrDefaultAsync(r => r.Code.ToUpper() == code.ToUpper() && !r.IsDeleted);
        }

        public async Task<IEnumerable<Core.Models.Route>> GetAllAsync(int skip = 0, int take = 100)
        {
            return await _context.Routes
                .Include(r => r.TransportMode)
                .Include(r => r.StartStation)
                .Include(r => r.EndStation)
                .Where(r => !r.IsDeleted)
                .OrderBy(r => r.Name)
                .Skip(skip)
                .Take(take)
                .ToListAsync();
        }

        public async Task<IEnumerable<Core.Models.Route>> GetByTransportModeAsync(Guid transportModeId)
        {
            return await _context.Routes
                .Include(r => r.TransportMode)
                .Include(r => r.StartStation)
                .Include(r => r.EndStation)
                .Where(r => r.TransportModeId == transportModeId && !r.IsDeleted)
                .OrderBy(r => r.Name)
                .ToListAsync();
        }

        public async Task<IEnumerable<Core.Models.Route>> GetRoutesBetweenStationsAsync(Guid startStationId, Guid endStationId)
        {
            return await _context.Routes
                .Include(r => r.TransportMode)
                .Include(r => r.StartStation)
                .Include(r => r.EndStation)
                .Include(r => r.RouteStations)
                    .ThenInclude(rs => rs.Station)
                .Where(r => (r.StartStationId == startStationId && r.EndStationId == endStationId) ||
                           r.RouteStations.Any(rs => rs.StationId == startStationId) &&
                           r.RouteStations.Any(rs => rs.StationId == endStationId) &&
                           !r.IsDeleted)
                .OrderBy(r => r.EstimatedDuration)
                .ToListAsync();
        }

        public async Task<IEnumerable<Core.Models.Route>> SearchRoutesAsync(string searchTerm)
        {
            return await _context.Routes
                .Include(r => r.TransportMode)
                .Include(r => r.StartStation)
                .Include(r => r.EndStation)
                .Where(r => (r.Name.Contains(searchTerm) ||
                            r.Code.Contains(searchTerm) ||
                            r.StartStation.Name.Contains(searchTerm) ||
                            r.EndStation.Name.Contains(searchTerm)) &&
                            !r.IsDeleted)
                .OrderBy(r => r.Name)
                .ToListAsync();
        }

        public async Task<Core.Models.Route> CreateAsync(Core.Models.Route route)
        {
            _context.Routes.Add(route);
            await _context.SaveChangesAsync();
            return route;
        }

        public async Task<Core.Models.Route> UpdateAsync(Core.Models.Route route)
        {
            route.UpdatedAt = DateTime.UtcNow;
            _context.Routes.Update(route);
            await _context.SaveChangesAsync();
            return route;
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var route = await GetByIdAsync(id);
            if (route == null) return false;

            route.IsDeleted = true;
            route.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ExistsAsync(string code)
        {
            return await _context.Routes
                .AnyAsync(r => r.Code.ToUpper() == code.ToUpper() && !r.IsDeleted);
        }

        public async Task<IEnumerable<Core.Models.Route>> GetActiveRoutesAsync()
        {
            return await _context.Routes
                .Include(r => r.TransportMode)
                .Include(r => r.StartStation)
                .Include(r => r.EndStation)
                .Where(r => r.IsActive && !r.IsDeleted)
                .OrderBy(r => r.Name)
                .ToListAsync();
        }

        public async Task<IEnumerable<RouteStation>> GetRouteStationsAsync(Guid routeId)
        {
            return await _context.RouteStations
                .Include(rs => rs.Station)
                .Where(rs => rs.RouteId == routeId)
                .OrderBy(rs => rs.StationOrder)
                .ToListAsync();
        }
    }
}