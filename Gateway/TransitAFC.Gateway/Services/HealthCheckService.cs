using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace TransitAFC.Gateway.Services
{
    public class MicroservicesHealthCheckService : IHealthCheck
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly ILogger<MicroservicesHealthCheckService> _logger;

        public MicroservicesHealthCheckService(HttpClient httpClient, IConfiguration configuration, ILogger<MicroservicesHealthCheckService> logger)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            var services = new Dictionary<string, string>
            {
                { "UserService", "https://localhost:7001/health" },
                { "BookingService", "https://localhost:7003/health" },
                { "PaymentService", "https://localhost:7004/health" },
                { "TicketService", "https://localhost:7005/health" }
            };

            var healthyServices = new List<string>();
            var unhealthyServices = new List<string>();

            foreach (var service in services)
            {
                try
                {
                    var response = await _httpClient.GetAsync(service.Value, cancellationToken);
                    if (response.IsSuccessStatusCode)
                    {
                        healthyServices.Add(service.Key);
                    }
                    else
                    {
                        unhealthyServices.Add($"{service.Key} ({response.StatusCode})");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Health check failed for {ServiceName}", service.Key);
                    unhealthyServices.Add($"{service.Key} (Exception: {ex.Message})");
                }
            }

            var data = new Dictionary<string, object>
            {
                { "HealthyServices", healthyServices },
                { "UnhealthyServices", unhealthyServices },
                { "TotalServices", services.Count },
                { "HealthyCount", healthyServices.Count },
                { "UnhealthyCount", unhealthyServices.Count }
            };

            if (unhealthyServices.Any())
            {
                return HealthCheckResult.Degraded($"{unhealthyServices.Count} of {services.Count} services are unhealthy", data: data);
            }

            return HealthCheckResult.Healthy($"All {services.Count} services are healthy", data);
        }
    }
}