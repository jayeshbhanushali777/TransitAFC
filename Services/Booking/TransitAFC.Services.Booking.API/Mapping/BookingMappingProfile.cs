using AutoMapper;
using TransitAFC.Services.Booking.Core.DTOs;
using TransitAFC.Services.Booking.Core.Models;

namespace TransitAFC.Services.Booking.API.Mapping
{
    public class BookingMappingProfile : Profile
    {
        public BookingMappingProfile()
        {
            // Booking mappings
            CreateMap<Core.Models.Booking, BookingResponse>()
                .ForMember(dest => dest.Route, opt => opt.MapFrom(src => new RouteInfo
                {
                    Id = src.RouteId,
                    Code = src.RouteCode,
                    Name = src.RouteName
                }))
                .ForMember(dest => dest.SourceStation, opt => opt.MapFrom(src => new StationInfo
                {
                    Id = src.SourceStationId,
                    Name = src.SourceStationName
                }))
                .ForMember(dest => dest.DestinationStation, opt => opt.MapFrom(src => new StationInfo
                {
                    Id = src.DestinationStationId,
                    Name = src.DestinationStationName
                }))
                .ForMember(dest => dest.SeatNumbers, opt => opt.MapFrom(src => ParseSeatNumbers(src.SeatNumbers)))
                .ForMember(dest => dest.Payment, opt => opt.Ignore())
                .ForMember(dest => dest.Ticket, opt => opt.Ignore())
                .ForMember(dest => dest.ReturnBooking, opt => opt.Ignore());

            CreateMap<CreateBookingRequest, Core.Models.Booking>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.BookingNumber, opt => opt.Ignore())
                .ForMember(dest => dest.UserId, opt => opt.Ignore())
                .ForMember(dest => dest.RouteCode, opt => opt.Ignore())
                .ForMember(dest => dest.RouteName, opt => opt.Ignore())
                .ForMember(dest => dest.SourceStationName, opt => opt.Ignore())
                .ForMember(dest => dest.DestinationStationName, opt => opt.Ignore())
                .ForMember(dest => dest.ArrivalTime, opt => opt.Ignore())
                .ForMember(dest => dest.Status, opt => opt.Ignore())
                .ForMember(dest => dest.PassengerCount, opt => opt.MapFrom(src => src.Passengers.Count))
                .ForMember(dest => dest.TotalFare, opt => opt.Ignore())
                .ForMember(dest => dest.DiscountAmount, opt => opt.Ignore())
                .ForMember(dest => dest.TaxAmount, opt => opt.Ignore())
                .ForMember(dest => dest.FinalAmount, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
                .ForMember(dest => dest.IsActive, opt => opt.Ignore())
                .ForMember(dest => dest.Passengers, opt => opt.Ignore())
                .ForMember(dest => dest.BookingHistory, opt => opt.Ignore());

            // Passenger mappings
            CreateMap<BookingPassenger, PassengerResponse>()
                .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => $"{src.FirstName} {src.LastName}"));

            CreateMap<CreatePassengerRequest, BookingPassenger>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.BookingId, opt => opt.Ignore())
                .ForMember(dest => dest.Booking, opt => opt.Ignore())
                .ForMember(dest => dest.SeatNumber, opt => opt.Ignore())
                .ForMember(dest => dest.Fare, opt => opt.Ignore())
                .ForMember(dest => dest.DiscountAmount, opt => opt.Ignore())
                .ForMember(dest => dest.FinalFare, opt => opt.Ignore())
                .ForMember(dest => dest.TicketNumber, opt => opt.Ignore())
                .ForMember(dest => dest.CheckInTime, opt => opt.Ignore())
                .ForMember(dest => dest.CheckOutTime, opt => opt.Ignore())
                .ForMember(dest => dest.SeatType, opt => opt.MapFrom(src => src.PreferredSeatType))
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
                .ForMember(dest => dest.IsActive, opt => opt.Ignore());
        }

        private static List<string> ParseSeatNumbers(string? seatNumbersJson)
        {
            if (string.IsNullOrEmpty(seatNumbersJson))
                return new List<string>();

            try
            {
                return System.Text.Json.JsonSerializer.Deserialize<List<string>>(seatNumbersJson) ?? new List<string>();
            }
            catch
            {
                return new List<string>();
            }
        }
    }
}