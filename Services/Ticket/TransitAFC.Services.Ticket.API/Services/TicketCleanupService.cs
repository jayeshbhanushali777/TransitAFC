using TransitAFC.Services.Ticket.API.Services;

namespace TransitAFC.Services.Ticket.API.Services
{
    public class TicketCleanupService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<TicketCleanupService> _logger;
        private readonly TimeSpan _interval = TimeSpan.FromMinutes(10); // Run every 10 minutes

        public TicketCleanupService(IServiceScopeFactory scopeFactory, ILogger<TicketCleanupService> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = _scopeFactory.CreateScope();
                    var ticketService = scope.ServiceProvider.GetRequiredService<ITicketService>();

                    await ticketService.ProcessExpiredTicketsAsync();

                    await Task.Delay(_interval, stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error in ticket cleanup service");
                    await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken); // Wait 1 minute before retry
                }
            }
        }
    }
}