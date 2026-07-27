using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Transportation.Application.Route.Commands;
using Transportation.Application.Route.Models;
using Transportation.Application.Route.Queries;
using Transportation.Mediator.Helper.Common.Models;
using Transportation.Shared.Authorization;

namespace Transportation.API.Controllers;

public class RouteController : BaseController
{
    public RouteController(IMediator mediator) : base(mediator)
    {
    }

    [HttpGet]
    [RoleAuthorize(RoleNames.Admin, RoleNames.RouteManager, RoleNames.Accountant)]
    public async Task<PaginatedResult<RouteDto>> GetAllAsync(
        [FromQuery] GetAllRoutes getAllRoutes,
        CancellationToken cancellationToken = default)
    {
        return await _mediator.Send(getAllRoutes, cancellationToken);
    }

    [HttpGet("{id:long}")]
    [RoleAuthorize(RoleNames.Admin, RoleNames.RouteManager, RoleNames.Accountant)]
    public async Task<RouteDto> GetByIdAsync(
            long id, CancellationToken ct = default
    )
    {
        return await _mediator.Send(new GetRouteById(id), ct);
    }

    [HttpPost]
    [RoleAuthorize(RoleNames.Admin, RoleNames.RouteManager)]
    public async Task<RouteDto> AddAsync(
        [FromBody] CreateRouteCommand createRouteCommand,
        CancellationToken ct = default
    )
    {
        return await _mediator.Send(createRouteCommand);
    }

    [HttpPut]
    [RoleAuthorize(RoleNames.Admin, RoleNames.RouteManager)]
    public async Task<NoContentResult> UpdateAsync(
        [FromBody] UpdateRouteCommand updateRouteCommand,
        CancellationToken ct = default
    )
    {
        await _mediator.Send(updateRouteCommand);
        return new NoContentResult();
    }
    
    [HttpDelete("{id:long}")]
    [RoleAuthorize(RoleNames.Admin, RoleNames.RouteManager)]
    public async Task<NoContentResult> DeleteAsync(
         long id, CancellationToken ct = default
    )
    {
        await _mediator.Send(new DeleteRouteCommand(id));
        return new NoContentResult();
    }
}