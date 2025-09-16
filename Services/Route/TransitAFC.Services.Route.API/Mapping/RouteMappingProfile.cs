using AutoMapper;
using TransitAFC.Services.Route.Core.DTOs;
using TransitAFC.Services.Route.Core.Models;

namespace TransitAFC.Services.Route.API.Mapping
{
    public class RouteMappingProfile : Profile
    {
        public RouteMappingProfile()
        {
            // Route mappings
            CreateMap<Core.Models.Route, RouteDetailsResponse>()
                .ForMember(dest => dest.TransportMode, opt => opt.MapFrom(src => src.TransportMode.Name))
                .ForMember(dest => dest.Stations, opt => opt.Ignore())
                .ForMember(dest => dest.Schedules, opt => opt.Ignore())
                .ForMember(dest => dest.RealTime, opt => opt.Ignore());

            CreateMap<CreateRouteRequest, Core.Models.Route>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => true))
                .ForMember(dest => dest.TransportMode, opt => opt.Ignore())
                .ForMember(dest => dest.StartStation, opt => opt.Ignore())
                .ForMember(dest => dest.EndStation, opt => opt.Ignore())
                .ForMember(dest => dest.RouteStations, opt => opt.Ignore())
                .ForMember(dest => dest.Schedules, opt => opt.Ignore());

            CreateMap<UpdateRouteRequest, Core.Models.Route>()
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

            // Station mappings
            CreateMap<Station, StationInfo>()
                .ForMember(dest => dest.TransportMode, opt => opt.MapFrom(src => src.TransportMode.Name))
                .ForMember(dest => dest.Amenities, opt => opt.MapFrom(src => ParseAmenities(src.Amenities)))
                .ForMember(dest => dest.IsAccessible, opt => opt.MapFrom(src => src.HasWheelchairAccess));

            CreateMap<Station, StationResponse>()
                .ForMember(dest => dest.TransportMode, opt => opt.MapFrom(src => src.TransportMode.Name))
                .ForMember(dest => dest.Amenities, opt => opt.MapFrom(src => ParseAmenities(src.Amenities)))
                .ForMember(dest => dest.ConnectedRoutes, opt => opt.Ignore());

            CreateMap<CreateStationRequest, Station>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.Location, opt => opt.Ignore())
                .ForMember(dest => dest.Amenities, opt => opt.MapFrom(src => SerializeAmenities(src.Amenities)))
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => true))
                .ForMember(dest => dest.LastUpdated, opt => opt.Ignore())
                .ForMember(dest => dest.TransportMode, opt => opt.Ignore())
                .ForMember(dest => dest.RouteStations, opt => opt.Ignore())
                .ForMember(dest => dest.Schedules, opt => opt.Ignore());

            // RouteStation mappings
            CreateMap<RouteStation, RouteStationInfo>()
                .ForMember(dest => dest.Order, opt => opt.MapFrom(src => src.StationOrder))
                .ForMember(dest => dest.TravelTimeFromPrevious, opt => opt.MapFrom(src => src.EstimatedTravelTime));
        }

        private static List<string> ParseAmenities(string? amenitiesJson)
        {
            if (string.IsNullOrEmpty(amenitiesJson))
                return new List<string>();

            try
            {
                return System.Text.Json.JsonSerializer.Deserialize<List<string>>(amenitiesJson) ?? new List<string>();
            }
            catch
            {
                return new List<string>();
            }
        }

        private static string SerializeAmenities(List<string> amenities)
        {
            try
            {
                return System.Text.Json.JsonSerializer.Serialize(amenities);
            }
            catch
            {
                return "[]";
            }
        }
    }
}