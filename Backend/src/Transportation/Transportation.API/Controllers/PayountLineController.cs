using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Transportation.API.Models;
using Transportation.Application.PayoutLine.Commands;
using Transportation.Application.PayoutLine.Models;
using Transportation.Application.PayoutLine.Queries;
using Transportation.Mediator.Helper.Common.Models;
using Transportation.Shared.Authorization;

namespace Transportation.API.Controllers;

[RoleAuthorize(RoleNames.Admin, RoleNames.Accountant)]
public class PayoutLineController : BaseController
{
    public PayoutLineController(IMediator mediator) : base(mediator)
    {
    }

    [HttpGet]
    public async Task<PaginatedResult<PayoutLineDto>> GetAll(
        [FromQuery] GetAllPayoutLines query,
        CancellationToken cancellationToken = default)
    {
        return await _mediator.Send(query, cancellationToken);
    }

    [HttpGet("{id:long}")]
    public async Task<PayoutLineDto> GetById(
        long id,
        CancellationToken cancellationToken = default)
    {
        return await _mediator.Send(new GetPayoutLineById(id), cancellationToken);
    }

    [HttpPost]
    public async Task<PayoutLineDto> Create(
        [FromBody] CreatePayoutLineCommand command,
        CancellationToken cancellationToken = default)
    {
        return await _mediator.Send(command, cancellationToken);
    }

    [HttpPut("{id:long}")]
    public async Task<PayoutLineDto> Update(
        long id,
        [FromBody] UpdatePayoutLineRequest request,
        CancellationToken cancellationToken = default)
    {
        var command = new UpdatePayoutLineCommand(
            id,
            request.DriverKm,
            request.ExtraBusinessKm,
            request.TaxiCompensation
        );

        return await _mediator.Send(command, cancellationToken);
    }

    [HttpPut("{id:long}")]
    public async Task<NoContentResult> MarkPaid(
        long id,
        CancellationToken cancellationToken = default)
    {
        await _mediator.Send(new MarkPayoutLinePaidCommand(id), cancellationToken);

        return new NoContentResult();
    }

    [HttpDelete("{id:long}")]
    public async Task<bool> Delete(
        long id,
        CancellationToken cancellationToken = default)
    {
        return await _mediator.Send(new DeletePayoutLineCommand(id), cancellationToken);
    }
}

