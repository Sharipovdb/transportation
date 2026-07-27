using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Transportation.API.Models;
using Transportation.Application.CrewMembership.Commands;
using Transportation.Application.CrewMembership.Models;
using Transportation.Application.CrewMembership.Queries;
using Transportation.Mediator.Helper.Common.Models;
using Transportation.Shared.Authorization;

namespace Transportation.API.Controllers;

public class CrewMembershipController : BaseController
{
    public CrewMembershipController(IMediator mediator) : base(mediator)
    {
    }

    [HttpGet]
    [RoleAuthorize(RoleNames.Admin)]
    public Task<PaginatedResult<CrewMembershipDto>> GetAll([FromQuery] GetAllCrewMembershipQuery query,
        CancellationToken token)
        => _mediator.Send(query, token);

    [HttpGet("{id:long}")]
    [Authorize(Roles = RoleNames.Worker)]
    public Task<CrewMembershipDto> GetById(long id, CancellationToken ct)
        => _mediator.Send(new GetByIdCrewMembershipQuery(id), ct);

    [HttpPost]
    [RoleAuthorize(RoleNames.Admin)]
    public async Task<ActionResult<List<CrewMembershipDto>>> Create(
        [FromBody] CreateCrewMembershipCommand command,
        CancellationToken token)
    {
        return await _mediator.Send(command, token);
    }

    [HttpPut("{id:long}")]
    [RoleAuthorize(RoleNames.Admin, RoleNames.RouteManager)]
    public async Task<ActionResult<CrewMembershipDto>> Update(
        long id, 
        [FromBody] UpdateCrewMembershipRequest request,
        CancellationToken token)
    {
        var command = new UpdateCrewMembershipCommand(
            Id: id,
            ActiveFrom: request.ActiveFrom,
            ActiveTo: request.ActiveTo
        );

        var result = await _mediator.Send(command, token);

        return Ok(result);
    }

    [HttpDelete("{id:long}")]
    [RoleAuthorize(RoleNames.Admin, RoleNames.RouteManager)]
    public async Task<IActionResult> Delete(long id, CancellationToken token)
    {
        await _mediator.Send(new DeleteCrewMembershipCommand(id), token);
        return NoContent();
    }

    [HttpPost("transfer")]
    [RoleAuthorize(RoleNames.Admin, RoleNames.RouteManager)]
    public async Task<ActionResult<CrewMembershipDto>> Transfer([FromBody] TransferCrewMembershipCommand command,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(command, cancellationToken);
        return Ok(result);
    }
}