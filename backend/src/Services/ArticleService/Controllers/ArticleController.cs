using MediatR;
using Microsoft.AspNetCore.Mvc;
using RealWorldApp.Shared.Contracts;

namespace RealWorldApp.ArticleService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ArticleController : ControllerBase
{
    private readonly IMediator _mediator;

    public ArticleController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    public async Task<ActionResult<ArticleDto>> Create(CreateArticleCommand command)
    {
        var result = await _mediator.Send(command);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, UpdateArticleCommand command)
    {
        if (id != command.Id)
            return BadRequest();

        var result = await _mediator.Send(command);
        return result ? Ok() : NotFound();
    }

/*
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var result = await _mediator.Send(new DeleteArticleCommand(id));
        return result ? Ok() : NotFound();
    }
*/

    [HttpGet("{id}")]
    public async Task<ActionResult<ArticleDto>> GetById(Guid id)
    {
        // This endpoint is just for demo - in real app, query service handles reads
        // We're redirecting to query service
        return Ok();
    }
}