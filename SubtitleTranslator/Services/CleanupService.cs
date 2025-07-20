namespace SubtitleTranslator.Services
{
    public class CleanupService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<CleanupService> _logger;
        private readonly TimeSpan _period = TimeSpan.FromHours(1); // Run every hour

        public CleanupService(IServiceProvider serviceProvider, ILogger<CleanupService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using (var scope = _serviceProvider.CreateScope())
                    {
                        var fileStorageService = scope.ServiceProvider.GetRequiredService<IFileStorageService>();

                        // Clean up files older than 24 hours
                        fileStorageService.CleanupOldFiles(TimeSpan.FromHours(24));
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error during cleanup service execution");
                }

                await Task.Delay(_period, stoppingToken);
            }
        }
    }
}