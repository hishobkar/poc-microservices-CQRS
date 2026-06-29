using RealWorldApp.Shared.Events;
using RealWorldApp.Shared.Infrastructure;
using RealWorldApp.QueryService.Data;
using RealWorldApp.QueryService.Models;

namespace RealWorldApp.QueryService.Consumers;

public class ArticleCreatedConsumer : KafkaConsumer<ArticleCreatedEvent>
{
    private readonly ILogger<ArticleCreatedConsumer> _logger;
    private readonly IServiceProvider _serviceProvider;

    public ArticleCreatedConsumer(
        IConfiguration configuration, 
        IServiceProvider serviceProvider,
        ILogger<ArticleCreatedConsumer> logger) 
        : base(configuration, serviceProvider, logger, KafkaTopics.ArticleCreated, "query-service-group")
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
    }

    protected override async Task HandleMessageAsync(ArticleCreatedEvent message, CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ReadDbContext>();

        _logger.LogInformation("Processing article creation: {ArticleId}", message.ArticleId);

        var readModel = new ArticleReadModel
        {
            Id = message.ArticleId,
            Title = message.Title,
            Content = message.Content,
            Author = message.Author,
            CreatedAt = DateTime.UtcNow,
            ViewCount = 0,
            LikeCount = 0
        };

        await dbContext.Articles.AddAsync(readModel, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}