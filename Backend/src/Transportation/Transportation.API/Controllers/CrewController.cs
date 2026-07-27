using MediatR;
using Microsoft.AspNetCore.Mvc;
using Transportation.Application.Crew.Commands;
using Transportation.Application.Crew.Models;
using Transportation.Application.Crew.Queries;
using Transportation.Mediator.Helper.Common.Models;
using Transportation.Shared.Authorization;

namespace Transportation.API.Controllers;

public class CrewController : BaseController
{
    public CrewController(IMediator mediator) : base(mediator)
    {
    }

    // Every other controller's GetAll is open to any authenticated user (crew leads,
    // drivers, and accountants all need to see crews for their day-to-day work) —
    // only the write operations below are restricted to Admin/RouteManager.
    [HttpGet]
    public Task<PaginatedResult<CrewDto>> GetAll([FromQuery] GetAllCrewQuery query, CancellationToken token)
        => _mediator.Send(query, token);

    [HttpGet("{id:long}")]
    public Task<CrewDto> GetById(long id, CancellationToken ct)
        => _mediator.Send(new GetCrewByIdQuery(id), ct);

    [HttpPost]
    [RoleAuthorize(RoleNames.RouteManager,RoleNames.Admin)]
    public Task<CrewDto> Create(
        [FromBody] CreateCrewCommand command,
        CancellationToken token)
        => _mediator.Send(command, token);


    [HttpPut("{id:long}")]
    [RoleAuthorize(RoleNames.RouteManager, RoleNames.Admin)]
    public async Task<ActionResult<CrewDto>> Update(long id,
        [FromBody] UpdateCrewCommand request,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(request, cancellationToken);

        return Ok(result);
    }

    [HttpDelete("{id:long}")]
    [RoleAuthorize(RoleNames.Admin,RoleNames.RouteManager)]
    public async Task<IActionResult> Delete(long id, CancellationToken token)
    {
        await _mediator.Send(new DeleteCrewCommand(id), token);
        return NoContent();
    }
}