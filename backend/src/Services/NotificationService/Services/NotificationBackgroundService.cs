namespace RealWorldApp.NotificationService.Services;

public class NotificationBackgroundService : BackgroundService
{
    private readonly ILogger<NotificationBackgroundService> _logger;

    public NotificationBackgroundService(ILogger<NotificationBackgroundService> logger)
    {
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Notification Background Service is starting");

        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);
            _logger.LogInformation("Notification Service is alive");
        }

        _logger.LogInformation("Notification Background Service is stopping");
    }
}
