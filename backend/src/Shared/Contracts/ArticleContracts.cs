using MediatR;

namespace RealWorldApp.Shared.Contracts;

public record CreateArticleCommand(string Title, string Content, string Author): IRequest<ArticleDto>;
public record UpdateArticleCommand(Guid Id, string Title, string Content): IRequest<bool>;
public record DeleteArticleCommand(Guid Id);

public record ArticleDto(Guid Id, string Title, string Content, string Author, DateTime CreatedAt);