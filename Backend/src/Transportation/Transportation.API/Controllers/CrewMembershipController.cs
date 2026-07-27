using MediatR;
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
    public CrewMembershipController(IMediator mediator) : base(mediator) {}

    // Same reasoning as CrewController.GetAll — reading membership is unrestricted,
    // only mutating it requires Admin/RouteManager.
    [HttpGet]
    public Task<PaginatedResult<CrewMembershipDto>> GetAll(
        [FromQuery] GetAllCrewMembershipQuery query, CancellationToken token)
        => _mediator.Send(query, token);

    [HttpGet("{id:long}")]
    public Task<CrewMembershipDto> GetById(long id, CancellationToken ct)
        => _mediator.Send(new GetByIdCrewMembershipQuery(id), ct);

    [HttpPost]
    [RoleAuthorize(RoleNames.Admin, RoleNames.RouteManager)]
    public Task<CrewMembershipDto> Create(
        [FromBody] CreateCrewMembershipCommand command,
        CancellationToken token)
        => _mediator.Send(command, token);
    
    [HttpPut("{id:long}")]
    [RoleAuthorize(RoleNames.Admin,RoleNames.RouteManager)]
    public async Task<ActionResult<CrewMembershipDto>> Update(
        long id, 
        [FromBody] UpdateCrewMembershipRequest request,
        CancellationToken token)
    {
        var command = new UpdateCrewMembership(
            Id: id,
            ActiveFrom: request.ActiveFrom,
            ActiveTo: request.ActiveTo
        );

        var result = await _mediator.Send(command, token);

        return Ok(result);
    }

    [HttpDelete("{id:long}")]
    [RoleAuthorize(RoleNames.Admin,RoleNames.RouteManager)]
    public async Task<IActionResult> Delete(long id, CancellationToken token)
    {
        await _mediator.Send(new DeleteCrewMembership(id), token);
        return NoContent();
    }

    [HttpPut("/api/CrewMembership/Transfer")] 
    [RoleAuthorize(RoleNames.Admin,RoleNames.RouteManager)]
    public async Task<ActionResult<CrewMembershipDto>> TransferCrewMembership(
        [FromBody] TransferCrewMembershipCommand command,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(command, cancellationToken);
    
        return Ok(result);
    }
}