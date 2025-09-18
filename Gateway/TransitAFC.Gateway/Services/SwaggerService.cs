namespace TransitAFC.Gateway.Services
{
    public class SwaggerService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<SwaggerService> _logger;

        public SwaggerService(HttpClient httpClient, ILogger<SwaggerService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task<string> GetServiceSwaggerAsync(string serviceName, string serviceUrl)
        {
            try
            {
                var response = await _httpClient.GetAsync($"{serviceUrl}/swagger/v1/swagger.json");
                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadAsStringAsync();
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to get Swagger for {ServiceName}", serviceName);
            }

            return "{}";
        }
    }
}