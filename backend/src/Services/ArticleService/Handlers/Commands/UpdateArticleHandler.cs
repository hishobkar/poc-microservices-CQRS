using MediatR;
using RealWorldApp.Shared.Contracts;
using RealWorldApp.Shared.Events;
using RealWorldApp.Shared.Infrastructure;
using RealWorldApp.ArticleService.Data;

namespace RealWorldApp.ArticleService.Handlers.Commands;

public class UpdateArticleHandler : IRequestHandler<UpdateArticleCommand, bool>
{
    private readonly ArticleDbContext _dbContext;
    private readonly IKafkaProducer _kafkaProducer;
    private readonly ILogger<UpdateArticleHandler> _logger;

    public UpdateArticleHandler(ArticleDbContext dbContext, IKafkaProducer kafkaProducer, ILogger<UpdateArticleHandler> logger)
    {
        _dbContext = dbContext;
        _kafkaProducer = kafkaProducer;
        _logger = logger;
    }

    public async Task<bool> Handle(UpdateArticleCommand request, CancellationToken cancellationToken)
    {
        var article = await _dbContext.Articles.FindAsync(new object[] { request.Id }, cancellationToken);

        if (article == null)
            return false;

        article.Title = request.Title;
        article.Content = request.Content;
        article.UpdatedAt = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync(cancellationToken);

        await _kafkaProducer.PublishAsync(
            KafkaTopics.ArticleUpdated,
            new ArticleUpdatedEvent(article.Id, article.Title, article.Content),
            article.Id.ToString()
        );

        _logger.LogInformation("Article updated: {ArticleId}", article.Id);

        return true;
    }
}
