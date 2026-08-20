using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Transportation.Application.Crew.Commands;
using Transportation.Application.Crew.Models;
using Transportation.Application.Crew.Queries;
using Transportation.Mediator.Helper.Common.Models;
using Transportation.Shared.Authorization;

namespace Transportation.API.Controllers;

public class CrewController : BaseController
{
    public CrewController(IMediator mediator) : base(mediator) {}

    [HttpGet]
    [RoleAuthorize(RoleNames.RouteManager, RoleNames.Admin, RoleNames.Accountant, RoleNames.CrewLead, RoleNames.DriverLead)]
    public Task<PaginatedResult<CrewDto>> GetAll([FromQuery] GetAllCrewQuery query, CancellationToken token)
        => _mediator.Send(query, token);

    [HttpGet("{id:long}")]
    [Authorize(Roles = RoleNames.Worker)]
    public Task<CrewDto> GetById(long id, CancellationToken ct)
        => _mediator.Send(new GetCrewByIdQuery(id), ct);

    [HttpPost]
    [RoleAuthorize(RoleNames.RouteManager, RoleNames.Admin)]
    public async Task<ActionResult<CrewDto>> Create([FromBody] CreateCrewCommand command, CancellationToken token)
    {
        var result = await _mediator.Send(command, token);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPut("{id:long}")]
    [RoleAuthorize(RoleNames.RouteManager, RoleNames.Admin)]
    public async Task<ActionResult<CrewDto>> Update(long id, [FromBody] UpdateCrewCommand request, CancellationToken cancellationToken)
    {
        var command = request with { Id = id };
        var result = await _mediator.Send(command, cancellationToken);

        return Ok(result);
    }

    [HttpDelete("{id:long}")]
    [RoleAuthorize(RoleNames.Admin, RoleNames.RouteManager)]
    public async Task<IActionResult> Delete(long id, CancellationToken token)
    {
        await _mediator.Send(new DeleteCrewCommand(id), token);
        return NoContent();
    }
}