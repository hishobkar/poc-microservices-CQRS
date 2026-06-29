using MediatR;
using Microsoft.AspNetCore.Mvc;
using RealWorldApp.Shared.Contracts;
using RealWorldApp.QueryService.Handlers.Queries;

namespace RealWorldApp.QueryService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class QueryController : ControllerBase
{
    private readonly IMediator _mediator;

    public QueryController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<ActionResult<List<ArticleDto>>> GetArticles([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        var query = new GetArticlesQuery(page, pageSize);
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ArticleDto>> GetArticleById(Guid id)
    {
        var query = new GetArticleByIdQuery(id);
        var result = await _mediator.Send(query);
        return result != null ? Ok(result) : NotFound();
    }
}