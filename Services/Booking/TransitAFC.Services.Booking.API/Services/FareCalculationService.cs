using TransitAFC.Services.Booking.API.Services;
using TransitAFC.Services.Booking.Core.DTOs;
using TransitAFC.Services.Booking.Core.Models;

namespace TransitAFC.Services.Booking.API.Services
{
    public class FareCalculationService : IFareCalculationService
    {
        private readonly IRouteService _routeService;
        private readonly IConfiguration _configuration;
        private readonly ILogger<FareCalculationService> _logger;

        public FareCalculationService(
            IRouteService routeService,
            IConfiguration configuration,
            ILogger<FareCalculationService> logger)
        {
            _routeService = routeService;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<FareCalculationResponse> CalculateFareAsync(FareCalculationRequest request)
        {
            try
            {
                // Get route info
                var routeInfo = await _routeService.GetRouteInfoAsync(request.RouteId, request.SourceStationId, request.DestinationStationId);
                if (routeInfo == null)
                {
                    throw new InvalidOperationException("Invalid route or stations");
                }

                var baseFare = routeInfo.BaseFare;
                var passengerFares = new List<PassengerFare>();
                decimal totalFare = 0;
                decimal totalDiscountAmount = 0;

                // Calculate fare for each passenger type
                foreach (var passengerType in request.PassengerTypes)
                {
                    var passengerBaseFare = baseFare;
                    var discountAmount = passengerBaseFare * GetPassengerTypeDiscount(passengerType);
                    var finalFare = passengerBaseFare - discountAmount;

                    passengerFares.Add(new PassengerFare
                    {
                        PassengerType = passengerType,
                        BaseFare = passengerBaseFare,
                        DiscountAmount = discountAmount,
                        FinalFare = finalFare,
                        DiscountReason = GetDiscountReason(passengerType)
                    });

                    totalFare += passengerBaseFare;
                    totalDiscountAmount += discountAmount;
                }

                // Apply discount code if provided
                decimal discountCodeAmount = 0;
                var appliedDiscounts = new List<string>();

                if (!string.IsNullOrEmpty(request.DiscountCode))
                {
                    var discountPercentage = GetDiscountCodeDiscount(request.DiscountCode);
                    if (discountPercentage > 0)
                    {
                        discountCodeAmount = (totalFare - totalDiscountAmount) * discountPercentage;
                        totalDiscountAmount += discountCodeAmount;
                        appliedDiscounts.Add(request.DiscountCode);
                    }
                }

                var subtotal = totalFare - totalDiscountAmount;
                var taxAmount = CalculateTax(subtotal);
                var finalAmount = subtotal + taxAmount;

                return new FareCalculationResponse
                {
                    BaseFare = baseFare,
                    PassengerFares = passengerFares,
                    TotalFare = totalFare,
                    DiscountAmount = totalDiscountAmount,
                    TaxAmount = taxAmount,
                    FinalAmount = finalAmount,
                    Currency = "INR",
                    AppliedDiscounts = appliedDiscounts,
                    Breakdown = new FareBreakdown
                    {
                        Distance = routeInfo.Distance,
                        RatePerKm = routeInfo.Distance > 0 ? baseFare / routeInfo.Distance : 0,
                        BaseFare = totalFare,
                        ServiceTax = taxAmount * 0.6m, // 60% of tax as service tax
                        PlatformFee = 2.00m,
                        ConvenienceFee = 1.50m,
                        TotalTax = taxAmount,
                        GrandTotal = finalAmount
                    }
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating fare for route {RouteId}", request.RouteId);
                throw;
            }
        }

        public decimal GetPassengerTypeDiscount(PassengerType passengerType)
        {
            return passengerType switch
            {
                PassengerType.Child => 0.50m,     // 50% discount
                PassengerType.Senior => 0.30m,    // 30% discount
                PassengerType.Student => 0.20m,   // 20% discount
                PassengerType.Disabled => 0.50m,  // 50% discount
                _ => 0.00m                         // No discount for adults
            };
        }

        public decimal GetDiscountCodeDiscount(string discountCode)
        {
            // This would typically check against a database of valid discount codes
            return discountCode.ToUpper() switch
            {
                "FIRST10" => 0.10m,    // 10% discount
                "STUDENT15" => 0.15m,  // 15% discount
                "SENIOR20" => 0.20m,   // 20% discount
                "WELCOME25" => 0.25m,  // 25% discount
                _ => 0.00m             // Invalid code
            };
        }

        public decimal CalculateTax(decimal amount)
        {
            // Calculate tax based on amount
            var taxRate = _configuration.GetValue<decimal>("Booking:TaxRate", 0.05m); // Default 5%
            return Math.Round(amount * taxRate, 2);
        }

        private static string GetDiscountReason(PassengerType passengerType)
        {
            return passengerType switch
            {
                PassengerType.Child => "Child Discount (50%)",
                PassengerType.Senior => "Senior Citizen Discount (30%)",
                PassengerType.Student => "Student Discount (20%)",
                PassengerType.Disabled => "Disability Discount (50%)",
                _ => ""
            };
        }
    }
}