using MediatR;
using RealWorldApp.Shared.Contracts;
using RealWorldApp.Shared.Events;
using RealWorldApp.Shared.Infrastructure;
using RealWorldApp.ArticleService.Data;
using RealWorldApp.ArticleService.Models;

namespace RealWorldApp.ArticleService.Handlers.Commands;

public class CreateArticleHandler : IRequestHandler<CreateArticleCommand, ArticleDto>
{
    private readonly ArticleDbContext _dbContext;
    private readonly IKafkaProducer _kafkaProducer;
    private readonly ILogger<CreateArticleHandler> _logger;

    public CreateArticleHandler(ArticleDbContext dbContext, IKafkaProducer kafkaProducer, ILogger<CreateArticleHandler> logger)
    {
        _dbContext = dbContext;
        _kafkaProducer = kafkaProducer;
        _logger = logger;
    }

    public async Task<ArticleDto> Handle(CreateArticleCommand request, CancellationToken cancellationToken)
    {
        var article = new Article
        {
            Id = Guid.NewGuid(),
            Title = request.Title,
            Content = request.Content,
            Author = request.Author,
            CreatedAt = DateTime.UtcNow
        };

        await _dbContext.Articles.AddAsync(article, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        // Publish event to Kafka
        var @event = new ArticleCreatedEvent(
            article.Id,
            article.Title,
            article.Content,
            article.Author
        );

        await _kafkaProducer.PublishAsync(KafkaTopics.ArticleCreated, @event, article.Id.ToString());

        _logger.LogInformation("Article created: {ArticleId}", article.Id);

        return new ArticleDto(
            article.Id,
            article.Title,
            article.Content,
            article.Author,
            article.CreatedAt
        );
    }
}