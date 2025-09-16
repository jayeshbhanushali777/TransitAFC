using Microsoft.EntityFrameworkCore;
using NetTopologySuite;
using NetTopologySuite.Geometries;
using TransitAFC.Services.Route.Core.Models;
using TransitAFC.Services.Route.Infrastructure.Repositories;

namespace TransitAFC.Services.Route.Infrastructure.Repositories
{
    public class StationRepository : IStationRepository
    {
        private readonly RouteDbContext _context;
        private readonly GeometryFactory _geometryFactory;

        public StationRepository(RouteDbContext context)
        {
            _context = context;
            _geometryFactory = NtsGeometryServices.Instance.CreateGeometryFactory(srid: 4326);
        }

        public async Task<Station?> GetByIdAsync(Guid id)
        {
            return await _context.Stations
                .Include(s => s.TransportMode)
                .FirstOrDefaultAsync(s => s.Id == id && !s.IsDeleted);
        }

        public async Task<Station?> GetByCodeAsync(string code)
        {
            return await _context.Stations
                .Include(s => s.TransportMode)
                .FirstOrDefaultAsync(s => s.Code.ToUpper() == code.ToUpper() && !s.IsDeleted);
        }

        public async Task<IEnumerable<Station>> GetAllAsync(int skip = 0, int take = 100)
        {
            return await _context.Stations
                .Include(s => s.TransportMode)
                .Where(s => !s.IsDeleted)
                .OrderBy(s => s.Name)
                .Skip(skip)
                .Take(take)
                .ToListAsync();
        }

        public async Task<IEnumerable<Station>> GetByTransportModeAsync(Guid transportModeId)
        {
            return await _context.Stations
                .Include(s => s.TransportMode)
                .Where(s => s.TransportModeId == transportModeId && !s.IsDeleted)
                .OrderBy(s => s.Name)
                .ToListAsync();
        }

        public async Task<IEnumerable<Station>> GetNearbyStationsAsync(double latitude, double longitude, double radiusKm)
        {
            var point = _geometryFactory.CreatePoint(new Coordinate(longitude, latitude));
            var radiusMeters = radiusKm * 1000;

            return await _context.Stations
                .Include(s => s.TransportMode)
                .Where(s => s.Location.Distance(point) <= radiusMeters && !s.IsDeleted)
                .OrderBy(s => s.Location.Distance(point))
                .ToListAsync();
        }

        public async Task<IEnumerable<Station>> SearchStationsAsync(string searchTerm)
        {
            return await _context.Stations
                .Include(s => s.TransportMode)
                .Where(s => (s.Name.Contains(searchTerm) ||
                            s.Code.Contains(searchTerm) ||
                            s.City.Contains(searchTerm) ||
                            s.Address.Contains(searchTerm)) &&
                            !s.IsDeleted)
                .OrderBy(s => s.Name)
                .ToListAsync();
        }

        public async Task<Station> CreateAsync(Station station)
        {
            station.Location = _geometryFactory.CreatePoint(new Coordinate(station.Longitude, station.Latitude));
            _context.Stations.Add(station);
            await _context.SaveChangesAsync();
            return station;
        }

        public async Task<Station> UpdateAsync(Station station)
        {
            station.UpdatedAt = DateTime.UtcNow;
            station.Location = _geometryFactory.CreatePoint(new Coordinate(station.Longitude, station.Latitude));
            station.LastUpdated = DateTime.UtcNow;
            _context.Stations.Update(station);
            await _context.SaveChangesAsync();
            return station;
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var station = await GetByIdAsync(id);
            if (station == null) return false;

            station.IsDeleted = true;
            station.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ExistsAsync(string code)
        {
            return await _context.Stations
                .AnyAsync(s => s.Code.ToUpper() == code.ToUpper() && !s.IsDeleted);
        }

        public async Task<IEnumerable<Station>> GetStationsByNameAsync(string name)
        {
            return await _context.Stations
                .Include(s => s.TransportMode)
                .Where(s => s.Name.ToLower().Contains(name.ToLower()) && !s.IsDeleted)
                .OrderBy(s => s.Name)
                .ToListAsync();
        }
    }
}