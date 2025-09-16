using TransitAFC.Services.Route.Core.DTOs;

namespace TransitAFC.Services.Route.API.Services
{
    public interface IStationService
    {
        Task<StationResponse?> GetStationByIdAsync(Guid stationId);
        Task<StationResponse?> GetStationByCodeAsync(string stationCode);
        Task<List<StationResponse>> GetAllStationsAsync(int skip = 0, int take = 100);
        Task<List<StationResponse>> GetStationsByTransportModeAsync(Guid transportModeId);
        Task<List<StationResponse>> SearchStationsAsync(string searchTerm);
        Task<List<StationInfo>> GetNearbyStationsAsync(NearbyStationsRequest request);
        Task<StationResponse> CreateStationAsync(CreateStationRequest request);
        Task<StationResponse> UpdateStationAsync(Guid stationId, UpdateStationRequest request);
        Task<bool> DeleteStationAsync(Guid stationId);
    }
}