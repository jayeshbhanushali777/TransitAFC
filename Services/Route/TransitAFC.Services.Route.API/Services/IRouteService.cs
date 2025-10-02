using TransitAFC.Services.Route.Core.DTOs;

namespace TransitAFC.Services.Route.API.Services
{
    public interface IRouteService
    {
        Task<RouteSearchResponse> SearchRoutesAsync(RouteSearchRequest request);
        Task<RouteSearchResponse> SmartRouteSearchAsync(SmartRouteRequest request);
        Task<RouteDetailsResponse?> GetRouteDetailsAsync(string routeCode, DateTime? date = null);
        Task<RouteDetailsResponse?> GetRouteDetailsByIdAsync(Guid routeId, DateTime? date = null);
        Task<List<StationInfo>> GetNearbyStationsAsync(NearbyStationsRequest request);
        Task<RouteDetailsResponse> CreateRouteAsync(CreateRouteRequest request);
        Task<RouteDetailsResponse> UpdateRouteAsync(Guid routeId, UpdateRouteRequest request);
        Task<bool> DeleteRouteAsync(Guid routeId);
        Task<List<RouteDetailsResponse>> GetAllRoutesAsync(int skip = 0, int take = 100);
    }
}