using AutoMapper;
using TransitAFC.Services.Route.API.Services;
using TransitAFC.Services.Route.Core.DTOs;
using TransitAFC.Services.Route.Core.Models;
using TransitAFC.Services.Route.Infrastructure.Repositories;

namespace TransitAFC.Services.Route.API.Services
{
    public class RouteService : IRouteService
    {
        private readonly IRouteRepository _routeRepository;
        private readonly IStationRepository _stationRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<RouteService> _logger;

        public RouteService(
            IRouteRepository routeRepository,
            IStationRepository stationRepository,
            IMapper mapper,
            ILogger<RouteService> logger)
        {
            _routeRepository = routeRepository;
            _stationRepository = stationRepository;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<RouteSearchResponse> SearchRoutesAsync(RouteSearchRequest request)
        {
            try
            {
                _logger.LogInformation("Searching routes from {Source} to {Destination}", request.Source, request.Destination);

                // Find source and destination stations
                var sourceStations = await FindStationsAsync(request.Source);
                var destinationStations = await FindStationsAsync(request.Destination);

                if (!sourceStations.Any())
                {
                    throw new InvalidOperationException($"Source station '{request.Source}' not found");
                }

                if (!destinationStations.Any())
                {
                    throw new InvalidOperationException($"Destination station '{request.Destination}' not found");
                }

                var routeOptions = new List<RouteOption>();

                // Search for direct routes
                foreach (var source in sourceStations)
                {
                    foreach (var destination in destinationStations)
                    {
                        var directRoutes = await _routeRepository.GetRoutesBetweenStationsAsync(source.Id, destination.Id);

                        foreach (var route in directRoutes)
                        {
                            var routeOption = await BuildRouteOptionAsync(route, source.Id, destination.Id, request.DepartureTime);
                            if (routeOption != null)
                            {
                                routeOptions.Add(routeOption);
                            }
                        }
                    }
                }

                // TODO: Add transfer route logic here

                // Sort routes based on preference
                if (request.PreferFastest)
                {
                    routeOptions = routeOptions.OrderBy(r => r.TotalDuration).ToList();
                }
                else
                {
                    routeOptions = routeOptions.OrderBy(r => r.TotalFare).ToList();
                }

                return new RouteSearchResponse
                {
                    Routes = routeOptions,
                    SourceStation = _mapper.Map<StationInfo>(sourceStations.First()),
                    DestinationStation = _mapper.Map<StationInfo>(destinationStations.First()),
                    SearchTime = DateTime.UtcNow,
                    SearchId = Guid.NewGuid().ToString()
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching routes from {Source} to {Destination}", request.Source, request.Destination);
                throw;
            }
        }

        public async Task<RouteSearchResponse> SmartRouteSearchAsync(SmartRouteRequest request)
        {
            // This would integrate with Google AI for intelligent route recommendations
            var basicRequest = new RouteSearchRequest
            {
                Source = request.Source,
                Destination = request.Destination,
                DepartureTime = request.PreferredDepartureTime,
                PreferFastest = true
            };

            var basicResult = await SearchRoutesAsync(basicRequest);

            // TODO: Enhance with AI-powered recommendations
            // - Consider weather conditions
            // - Factor in crowd levels
            // - Personalize based on user preferences
            // - Apply dynamic pricing

            return basicResult;
        }

        public async Task<RouteDetailsResponse?> GetRouteDetailsAsync(string routeCode, DateTime? date = null)
        {
            var route = await _routeRepository.GetByCodeAsync(routeCode);
            if (route == null) return null;

            var routeStations = await _routeRepository.GetRouteStationsAsync(route.Id);

            var response = _mapper.Map<RouteDetailsResponse>(route);
            response.Stations = _mapper.Map<List<RouteStationInfo>>(routeStations);

            // TODO: Add schedule and real-time information

            return response;
        }

        public async Task<List<StationInfo>> GetNearbyStationsAsync(NearbyStationsRequest request)
        {
            var stations = await _stationRepository.GetNearbyStationsAsync(
                request.Latitude,
                request.Longitude,
                request.RadiusKm);

            return _mapper.Map<List<StationInfo>>(stations.Take(request.MaxResults));
        }

        public async Task<RouteDetailsResponse> CreateRouteAsync(CreateRouteRequest request)
        {
            // Validate that route code doesn't exist
            if (await _routeRepository.ExistsAsync(request.Code))
            {
                throw new InvalidOperationException($"Route with code '{request.Code}' already exists");
            }

            var route = _mapper.Map<Core.Models.Route>(request);
            var createdRoute = await _routeRepository.CreateAsync(route);

            return _mapper.Map<RouteDetailsResponse>(createdRoute);
        }

        public async Task<RouteDetailsResponse> UpdateRouteAsync(Guid routeId, UpdateRouteRequest request)
        {
            var route = await _routeRepository.GetByIdAsync(routeId);
            if (route == null)
            {
                throw new InvalidOperationException("Route not found");
            }

            _mapper.Map(request, route);
            var updatedRoute = await _routeRepository.UpdateAsync(route);

            return _mapper.Map<RouteDetailsResponse>(updatedRoute);
        }

        public async Task<bool> DeleteRouteAsync(Guid routeId)
        {
            return await _routeRepository.DeleteAsync(routeId);
        }

        public async Task<List<RouteDetailsResponse>> GetAllRoutesAsync(int skip = 0, int take = 100)
        {
            var routes = await _routeRepository.GetAllAsync(skip, take);
            return _mapper.Map<List<RouteDetailsResponse>>(routes);
        }

        private async Task<List<Station>> FindStationsAsync(string searchTerm)
        {
            // Try to find by code first
            var stationByCode = await _stationRepository.GetByCodeAsync(searchTerm);
            if (stationByCode != null)
            {
                return new List<Station> { stationByCode };
            }

            // Then search by name
            var stationsByName = await _stationRepository.GetStationsByNameAsync(searchTerm);
            return stationsByName.ToList();
        }

        private async Task<RouteOption?> BuildRouteOptionAsync(Core.Models.Route route, Guid sourceStationId, Guid destinationStationId, DateTime? departureTime)
        {
            try
            {
                var routeStations = await _routeRepository.GetRouteStationsAsync(route.Id);

                var sourceRouteStation = routeStations.FirstOrDefault(rs => rs.StationId == sourceStationId);
                var destinationRouteStation = routeStations.FirstOrDefault(rs => rs.StationId == destinationStationId);

                if (sourceRouteStation == null || destinationRouteStation == null)
                    return null;

                var segment = new RouteSegment
                {
                    RouteId = route.Id,
                    RouteName = route.Name,
                    RouteCode = route.Code,
                    TransportMode = route.TransportMode.Name,
                    StartStation = _mapper.Map<StationInfo>(sourceRouteStation.Station),
                    EndStation = _mapper.Map<StationInfo>(destinationRouteStation.Station),
                    DepartureTime = departureTime ?? DateTime.Now,
                    ArrivalTime = (departureTime ?? DateTime.Now).Add(route.EstimatedDuration),
                    Duration = route.EstimatedDuration,
                    Distance = Math.Abs(destinationRouteStation.DistanceFromStart - sourceRouteStation.DistanceFromStart),
                    Fare = Math.Abs(destinationRouteStation.FareFromStart - sourceRouteStation.FareFromStart),
                    Color = route.RouteColor
                };

                return new RouteOption
                {
                    RouteId = route.Id.ToString(),
                    Segments = new List<RouteSegment> { segment },
                    TotalFare = segment.Fare,
                    TotalDuration = segment.Duration,
                    TotalDistance = segment.Distance,
                    TransferCount = 0,
                    DepartureTime = segment.DepartureTime,
                    ArrivalTime = segment.ArrivalTime,
                    RouteType = "Direct",
                    IsAccessible = sourceRouteStation.Station.HasWheelchairAccess && destinationRouteStation.Station.HasWheelchairAccess,
                    ComfortScore = CalculateComfortScore(route)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error building route option for route {RouteId}", route.Id);
                return null;
            }
        }

        private static int CalculateComfortScore(Core.Models.Route route)
        {
            // Simple comfort scoring algorithm
            int score = 3; // Base score

            if (route.TransportMode.Name == "Metro") score += 2;
            else if (route.TransportMode.Name == "Train") score += 1;

            if (route.IsExpress) score += 1;
            if (route.FrequencyMinutes <= 10) score += 1;

            return Math.Min(5, Math.Max(1, score));
        }
    }
}