using Microsoft.AspNetCore.Mvc;
using TransitAFC.Gateway.Services;

namespace TransitAFC.Gateway.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DocsController : ControllerBase
    {
        private readonly SwaggerService _swaggerService;
        private readonly ILogger<DocsController> _logger;

        public DocsController(SwaggerService swaggerService, ILogger<DocsController> logger)
        {
            _swaggerService = swaggerService;
            _logger = logger;
        }

        [HttpGet("services")]
        public IActionResult GetServices()
        {
            var services = new[]
            {
                new { Name = "User Service", Url = "https://localhost:7001/swagger", Description = "User management and authentication" },
                new { Name = "Route Service", Url = "https://localhost:7002/swagger", Description = "Route management" },
                new { Name = "Booking Service", Url = "https://localhost:7003/swagger", Description = "Booking management" },
                new { Name = "Payment Service", Url = "https://localhost:7004/swagger", Description = "Payment processing" },
                new { Name = "Ticket Service", Url = "https://localhost:7005/swagger", Description = "Ticket generation and validation" }
            };

            return Ok(new
            {
                Gateway = "Transit AFC API Gateway",
                Version = "1.0.0",
                Services = services,
                Documentation = new
                {
                    Gateway = "/swagger",
                    HealthChecks = "/health-ui",
                    ApiHealth = "/api/health"
                }
            });
        }

        [HttpGet("swagger/{serviceName}")]
        public async Task<IActionResult> GetServiceSwagger(string serviceName)
        {
            var serviceUrls = new Dictionary<string, string>
            {
                { "users", "https://localhost:7001" },
                { "routes", "https://localhost:7002" },
                { "bookings", "https://localhost:7003" },
                { "payments", "https://localhost:7004" },
                { "tickets", "https://localhost:7005" }
            };

            if (!serviceUrls.TryGetValue(serviceName.ToLower(), out var serviceUrl))
            {
                return NotFound($"Service '{serviceName}' not found");
            }

            var swagger = await _swaggerService.GetServiceSwaggerAsync(serviceName, serviceUrl);
            return Content(swagger, "application/json");
        }
    }
}