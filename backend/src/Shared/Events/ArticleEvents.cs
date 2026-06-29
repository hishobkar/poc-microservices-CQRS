namespace RealWorldApp.Shared.Events;

public record ArticleCreatedEvent(Guid ArticleId, string Title, string Content, string Author);
public record ArticleUpdatedEvent(Guid ArticleId, string Title, string Content);
public record ArticleDeletedEvent(Guid ArticleId);