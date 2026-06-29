using RealWorldApp.NotificationService.Contracts;

namespace RealWorldApp.NotificationService.Services;

public class NotificationService : INotificationService
{
    private readonly IEmailService _emailService;
    private readonly ILogger<NotificationService> _logger;

    public NotificationService(IEmailService emailService, ILogger<NotificationService> logger)
    {
        _emailService = emailService;
        _logger = logger;
    }

    public async Task NotifyArticleCreatedAsync(Guid articleId, string title, string author)
    {
        var message = $"New article '{title}' was created by {author}";
        _logger.LogInformation(message);
        
        // In real app, send emails or push notifications
        await _emailService.SendEmailAsync("admin@example.com", "New Article Created", message);
    }

    public async Task NotifyArticleUpdatedAsync(Guid articleId, string title)
    {
        var message = $"Article '{title}' was updated";
        _logger.LogInformation(message);
        
        await _emailService.SendEmailAsync("admin@example.com", "Article Updated", message);
    }
}