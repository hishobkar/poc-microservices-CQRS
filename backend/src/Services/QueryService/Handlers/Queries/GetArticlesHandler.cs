using MediatR;
using Microsoft.EntityFrameworkCore;
using RealWorldApp.Shared.Contracts;
using RealWorldApp.QueryService.Data;

namespace RealWorldApp.QueryService.Handlers.Queries;

public record GetArticlesQuery(int Page = 1, int PageSize = 10) : IRequest<List<ArticleDto>>;
public record GetArticleByIdQuery(Guid Id) : IRequest<ArticleDto?>;

public class GetArticlesHandler : IRequestHandler<GetArticlesQuery, List<ArticleDto>>
{
    private readonly ReadDbContext _dbContext;

    public GetArticlesHandler(ReadDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<List<ArticleDto>> Handle(GetArticlesQuery request, CancellationToken cancellationToken)
    {
        var articles = await _dbContext.Articles
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(a => new ArticleDto(
                a.Id,
                a.Title,
                a.Content,
                a.Author,
                a.CreatedAt
            ))
            .ToListAsync(cancellationToken);

        return articles;
    }
}

public class GetArticleByIdHandler : IRequestHandler<GetArticleByIdQuery, ArticleDto?>
{
    private readonly ReadDbContext _dbContext;

    public GetArticleByIdHandler(ReadDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<ArticleDto?> Handle(GetArticleByIdQuery request, CancellationToken cancellationToken)
    {
        var article = await _dbContext.Articles
            .Where(a => a.Id == request.Id)
            .Select(a => new ArticleDto(
                a.Id,
                a.Title,
                a.Content,
                a.Author,
                a.CreatedAt
            ))
            .FirstOrDefaultAsync(cancellationToken);

        return article;
    }
}