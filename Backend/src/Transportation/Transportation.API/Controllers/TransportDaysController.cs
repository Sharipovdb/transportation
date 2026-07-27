using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Transportation.API.Models;
using Transportation.Application.TransportDay.Commands;
using Transportation.Application.TransportDay.Models;
using Transportation.Application.TransportDay.Queries;
using Transportation.Mediator.Helper.Common.Models;
using Transportation.Shared.Authorization;

namespace Transportation.API.Controllers;

public class TransportDaysController : BaseController
{
    public TransportDaysController(IMediator mediator) : base(mediator)
    {
    }

    [HttpGet]
    [RoleAuthorize(RoleNames.Admin, RoleNames.Accountant, RoleNames.RouteManager)]
    public async Task<PaginatedResult<TransportDayDto>> GetAll(
        [FromQuery] GetAllTransportDays query,
        CancellationToken cancellationToken = default)
    {
        return await _mediator.Send(query, cancellationToken);
    }

    [HttpGet("{id:long}")]
    [RoleAuthorize(RoleNames.Admin, RoleNames.Accountant, RoleNames.RouteManager)]
    public async Task<TransportDayDto> GetById(
        long id,
        CancellationToken cancellationToken = default)
    {
        return await _mediator.Send(new GetTransportDayById(id), cancellationToken);
    }

    [HttpPost]
    [RoleAuthorize(RoleNames.Admin, RoleNames.CrewLead, RoleNames.DriverLead)]
    public async Task<TransportDayDto> Create(
        [FromBody] CreateTransportDayCommand command,
        CancellationToken cancellationToken = default)
    {
        return await _mediator.Send(command, cancellationToken);
    }

    [HttpPut("{id:long}")]
    [RoleAuthorize(RoleNames.Admin, RoleNames.CrewLead, RoleNames.DriverLead)]
    public async Task<NoContentResult> Update(
        long id,
        [FromBody] UpdateTransportDayRequest request,
        CancellationToken cancellationToken = default)
    {
        await _mediator.Send(
            new UpdateTransportDayCommand(
                id,
                request.MorningMode,
                request.AfternoonMode,
                request.CommuteKm,
                request.ExtraBusinessKm,
                request.Notes
            ), cancellationToken);

        return new NoContentResult();
    }

    [HttpDelete("{id:long}")]
    [RoleAuthorize(RoleNames.Admin, RoleNames.CrewLead, RoleNames.DriverLead)]
    public async Task<NoContentResult> Delete(
        long id,
        CancellationToken cancellationToken = default)
    {
        await _mediator.Send(new DeleteTransportDayCommand(id), cancellationToken);

        return new NoContentResult();
    }
   
    [HttpPost("{id:long}/confirm")]
    [RoleAuthorize(RoleNames.Admin, RoleNames.CrewLead, RoleNames.DriverLead)]
    public async Task<TransportDayDto> Confirm(
        long id,
        CancellationToken cancellationToken)
    {
        return await _mediator.Send(
            new ConfirmTransportDayCommand(id),
            cancellationToken);
    }
    
    [HttpPost("{id:long}/unconfirm")]
    [RoleAuthorize(RoleNames.Admin, RoleNames.CrewLead, RoleNames.DriverLead)]
    public async Task<TransportDayDto> UnConfirm(
        long id,
        CancellationToken cancellationToken)
    {
        return await _mediator.Send(
            new UnConfirmTransportDayCommand(id),
            cancellationToken);
    }
}