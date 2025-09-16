using Microsoft.Extensions.DependencyInjection;
using TransitAFC.Services.Payment.Core.Models;

namespace TransitAFC.Services.Payment.Infrastructure.Gateways
{
    public interface IPaymentGatewayFactory
    {
        IPaymentGateway GetGateway(PaymentGateway gateway);
        IPaymentGateway GetBestGateway(PaymentMethod method, decimal amount);
        List<PaymentGateway> GetSupportedGateways(PaymentMethod method);
    }

    public class PaymentGatewayFactory : IPaymentGatewayFactory
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly Dictionary<PaymentGateway, Type> _gatewayTypes;

        public PaymentGatewayFactory(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            _gatewayTypes = new Dictionary<PaymentGateway, Type>
            {
                { PaymentGateway.Razorpay, typeof(RazorpayGateway) },
                { PaymentGateway.Stripe, typeof(StripeGateway) }
            };
        }

        public IPaymentGateway GetGateway(PaymentGateway gateway)
        {
            if (!_gatewayTypes.TryGetValue(gateway, out var gatewayType))
            {
                throw new NotSupportedException($"Payment gateway {gateway} is not supported");
            }

            return (IPaymentGateway)_serviceProvider.GetRequiredService(gatewayType);
        }

        public IPaymentGateway GetBestGateway(PaymentMethod method, decimal amount)
        {
            // Logic to select the best gateway based on method, amount, and fees
            // For India-based transit system, prefer Razorpay for UPI and local methods
            return method switch
            {
                PaymentMethod.UPI => GetGateway(PaymentGateway.Razorpay),
                PaymentMethod.NetBanking => GetGateway(PaymentGateway.Razorpay),
                PaymentMethod.Wallet => GetGateway(PaymentGateway.Razorpay),
                PaymentMethod.CreditCard when amount < 10000 => GetGateway(PaymentGateway.Razorpay),
                PaymentMethod.DebitCard when amount < 10000 => GetGateway(PaymentGateway.Razorpay),
                PaymentMethod.CreditCard => GetGateway(PaymentGateway.Stripe),
                PaymentMethod.DebitCard => GetGateway(PaymentGateway.Stripe),
                _ => GetGateway(PaymentGateway.Razorpay)
            };
        }

        public List<PaymentGateway> GetSupportedGateways(PaymentMethod method)
        {
            var supportedGateways = new List<PaymentGateway>();

            foreach (var (gateway, _) in _gatewayTypes)
            {
                try
                {
                    var gatewayInstance = GetGateway(gateway);
                    if (gatewayInstance.IsPaymentMethodSupported(method))
                    {
                        supportedGateways.Add(gateway);
                    }
                }
                catch
                {
                    // Gateway not available, skip
                }
            }

            return supportedGateways;
        }
    }
}