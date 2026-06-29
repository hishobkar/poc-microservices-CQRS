using RealWorldApp.Shared.Events;
using RealWorldApp.Shared.Infrastructure;
using RealWorldApp.NotificationService.Contracts;

namespace RealWorldApp.NotificationService.Consumers;

public class ArticleUpdatedNotificationConsumer : KafkaConsumer<ArticleUpdatedEvent>
{
    private readonly ILogger<ArticleUpdatedNotificationConsumer> _logger;

    public ArticleUpdatedNotificationConsumer(
        IConfiguration configuration,
        IServiceProvider serviceProvider,
        ILogger<ArticleUpdatedNotificationConsumer> logger)
        : base(configuration, serviceProvider, logger, KafkaTopics.ArticleUpdated, "notification-service-updated-group")
    {
        _logger = logger;
    }

    protected override async Task HandleMessageAsync(ArticleUpdatedEvent message, CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var notificationService = scope.ServiceProvider.GetRequiredService<INotificationService>();

        _logger.LogInformation("Sending notification for updated article: {Title}", message.Title);

        await notificationService.NotifyArticleUpdatedAsync(
            message.ArticleId,
            message.Title
        );
    }
}
