using RealWorldApp.Shared.Events;
using RealWorldApp.Shared.Infrastructure;
using RealWorldApp.NotificationService.Services;
using RealWorldApp.NotificationService.Contracts;

namespace RealWorldApp.NotificationService.Consumers;

public class ArticleCreatedNotificationConsumer : KafkaConsumer<ArticleCreatedEvent>
{
    private readonly ILogger<ArticleCreatedNotificationConsumer> _logger;

    public ArticleCreatedNotificationConsumer(
        IConfiguration configuration,
        IServiceProvider serviceProvider,
        ILogger<ArticleCreatedNotificationConsumer> logger)
        : base(configuration, serviceProvider, logger, KafkaTopics.ArticleCreated, "notification-service-group")
    {
        _logger = logger;
    }

    protected override async Task HandleMessageAsync(ArticleCreatedEvent message, CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var notificationService = scope.ServiceProvider.GetRequiredService<INotificationService>();

        _logger.LogInformation("Sending notification for new article: {Title}", message.Title);
        
        await notificationService.NotifyArticleCreatedAsync(
            message.ArticleId,
            message.Title,
            message.Author
        );
    }
}