using AutoMapper;
using TransitAFC.Services.Route.API.Services;
using TransitAFC.Services.Route.Core.DTOs;
using TransitAFC.Services.Route.Core.Models;
using TransitAFC.Services.Route.Infrastructure.Repositories;

namespace TransitAFC.Services.Route.API.Services
{
    public class StationService : IStationService
    {
        private readonly IStationRepository _stationRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<StationService> _logger;

        public StationService(
            IStationRepository stationRepository,
            IMapper mapper,
            ILogger<StationService> logger)
        {
            _stationRepository = stationRepository;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<StationResponse?> GetStationByIdAsync(Guid stationId)
        {
            try
            {
                var station = await _stationRepository.GetByIdAsync(stationId);
                return station != null ? _mapper.Map<StationResponse>(station) : null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting station by ID {StationId}", stationId);
                throw;
            }
        }

        public async Task<StationResponse?> GetStationByCodeAsync(string stationCode)
        {
            try
            {
                var station = await _stationRepository.GetByCodeAsync(stationCode);
                return station != null ? _mapper.Map<StationResponse>(station) : null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting station by code {StationCode}", stationCode);
                throw;
            }
        }

        public async Task<List<StationResponse>> GetAllStationsAsync(int skip = 0, int take = 100)
        {
            try
            {
                var stations = await _stationRepository.GetAllAsync(skip, take);
                return _mapper.Map<List<StationResponse>>(stations);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all stations");
                throw;
            }
        }

        public async Task<List<StationResponse>> GetStationsByTransportModeAsync(Guid transportModeId)
        {
            try
            {
                var stations = await _stationRepository.GetByTransportModeAsync(transportModeId);
                return _mapper.Map<List<StationResponse>>(stations);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting stations by transport mode {TransportModeId}", transportModeId);
                throw;
            }
        }

        public async Task<List<StationResponse>> SearchStationsAsync(string searchTerm)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(searchTerm))
                {
                    return new List<StationResponse>();
                }

                var stations = await _stationRepository.SearchStationsAsync(searchTerm);
                return _mapper.Map<List<StationResponse>>(stations);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching stations with term {SearchTerm}", searchTerm);
                throw;
            }
        }

        public async Task<List<StationInfo>> GetNearbyStationsAsync(NearbyStationsRequest request)
        {
            try
            {
                var stations = await _stationRepository.GetNearbyStationsAsync(
                    request.Latitude,
                    request.Longitude,
                    request.RadiusKm);

                var filteredStations = stations.Take(request.MaxResults);

                if (!string.IsNullOrEmpty(request.TransportMode))
                {
                    filteredStations = filteredStations.Where(s =>
                        s.TransportMode.Code.Equals(request.TransportMode, StringComparison.OrdinalIgnoreCase));
                }

                return _mapper.Map<List<StationInfo>>(filteredStations);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting nearby stations");
                throw;
            }
        }

        public async Task<StationResponse> CreateStationAsync(CreateStationRequest request)
        {
            try
            {
                // Check if station code already exists
                if (await _stationRepository.ExistsAsync(request.Code))
                {
                    throw new InvalidOperationException($"Station with code '{request.Code}' already exists");
                }

                var station = _mapper.Map<Station>(request);
                var createdStation = await _stationRepository.CreateAsync(station);

                _logger.LogInformation("Station created successfully: {StationCode}", createdStation.Code);
                return _mapper.Map<StationResponse>(createdStation);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating station with code {StationCode}", request.Code);
                throw;
            }
        }

        public async Task<StationResponse> UpdateStationAsync(Guid stationId, UpdateStationRequest request)
        {
            try
            {
                var existingStation = await _stationRepository.GetByIdAsync(stationId);
                if (existingStation == null)
                {
                    throw new InvalidOperationException("Station not found");
                }

                // Update only provided fields
                if (!string.IsNullOrEmpty(request.Name))
                    existingStation.Name = request.Name;

                if (!string.IsNullOrEmpty(request.Address))
                    existingStation.Address = request.Address;

                if (request.Latitude.HasValue)
                    existingStation.Latitude = request.Latitude.Value;

                if (request.Longitude.HasValue)
                    existingStation.Longitude = request.Longitude.Value;

                if (!string.IsNullOrEmpty(request.StationType))
                    existingStation.StationType = request.StationType;

                if (request.HasWheelchairAccess.HasValue)
                    existingStation.HasWheelchairAccess = request.HasWheelchairAccess.Value;

                if (request.HasParking.HasValue)
                    existingStation.HasParking = request.HasParking.Value;

                if (request.HasWiFi.HasValue)
                    existingStation.HasWiFi = request.HasWiFi.Value;

                if (request.HasRestroom.HasValue)
                    existingStation.HasRestroom = request.HasRestroom.Value;

                if (request.Amenities != null)
                    existingStation.Amenities = System.Text.Json.JsonSerializer.Serialize(request.Amenities);

                if (request.PlatformCount.HasValue)
                    existingStation.PlatformCount = request.PlatformCount.Value;

                if (request.IsActive.HasValue)
                    existingStation.IsActive = request.IsActive.Value;

                var updatedStation = await _stationRepository.UpdateAsync(existingStation);

                _logger.LogInformation("Station updated successfully: {StationId}", stationId);
                return _mapper.Map<StationResponse>(updatedStation);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating station {StationId}", stationId);
                throw;
            }
        }

        public async Task<bool> DeleteStationAsync(Guid stationId)
        {
            try
            {
                var result = await _stationRepository.DeleteAsync(stationId);

                if (result)
                {
                    _logger.LogInformation("Station deleted successfully: {StationId}", stationId);
                }
                else
                {
                    _logger.LogWarning("Station not found for deletion: {StationId}", stationId);
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting station {StationId}", stationId);
                throw;
            }
        }
    }
}