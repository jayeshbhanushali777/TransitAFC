using TransitAFC.Services.Booking.Core.DTOs;
using TransitAFC.Services.Booking.Core.Models;

namespace TransitAFC.Services.Booking.API.Services
{
    public interface IFareCalculationService
    {
        Task<FareCalculationResponse> CalculateFareAsync(FareCalculationRequest request);
        decimal GetPassengerTypeDiscount(PassengerType passengerType);
        decimal GetDiscountCodeDiscount(string discountCode);
        decimal CalculateTax(decimal amount);
    }
}