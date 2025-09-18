using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace TransitAFC.Gateway.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class HealthController : ControllerBase
    {
        private readonly HealthCheckService _healthCheckService;
        private readonly ILogger<HealthController> _logger;

        public HealthController(HealthCheckService healthCheckService, ILogger<HealthController> logger)
        {
            _healthCheckService = healthCheckService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> GetHealth()
        {
            var healthReport = await _healthCheckService.CheckHealthAsync();

            var response = new
            {
                Status = healthReport.Status.ToString(),
                TotalDuration = healthReport.TotalDuration,
                Results = healthReport.Entries.Select(x => new
                {
                    Key = x.Key,
                    Status = x.Value.Status.ToString(),
                    Duration = x.Value.Duration,
                    Description = x.Value.Description,
                    Data = x.Value.Data
                })
            };

            return healthReport.Status == HealthStatus.Healthy ? Ok(response) : StatusCode(503, response);
        }

        [HttpGet("services")]
        public async Task<IActionResult> GetServicesHealth()
        {
            var services = new[]
            {
                new { Name = "User Service", Url = "https://localhost:7001/health" },
                new { Name = "Booking Service", Url = "https://localhost:7003/health" },
                new { Name = "Payment Service", Url = "https://localhost:7004/health" },
                new { Name = "Ticket Service", Url = "https://localhost:7005/health" }
            };

            var results = new List<object>();

            using var httpClient = new HttpClient();
            httpClient.Timeout = TimeSpan.FromSeconds(10);

            foreach (var service in services)
            {
                try
                {
                    var start = DateTime.UtcNow;
                    var response = await httpClient.GetAsync(service.Url);
                    var duration = DateTime.UtcNow - start;

                    results.Add(new
                    {
                        Service = service.Name,
                        Status = response.IsSuccessStatusCode ? "Healthy" : "Unhealthy",
                        StatusCode = (int)response.StatusCode,
                        Duration = $"{duration.TotalMilliseconds}ms",
                        Url = service.Url
                    });
                }
                catch (Exception ex)
                {
                    results.Add(new
                    {
                        Service = service.Name,
                        Status = "Unreachable",
                        Error = ex.Message,
                        Url = service.Url
                    });
                }
            }

            return Ok(new
            {
                Timestamp = DateTime.UtcNow,
                Services = results,
                Summary = new
                {
                    Total = services.Length,
                    Healthy = results.Count(r => r.GetType().GetProperty("Status")?.GetValue(r)?.ToString() == "Healthy"),
                    Unhealthy = results.Count(r => r.GetType().GetProperty("Status")?.GetValue(r)?.ToString() != "Healthy")
                }
            });
        }
    }
}