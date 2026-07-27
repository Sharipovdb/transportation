using MediatR;
using Microsoft.AspNetCore.Mvc;
using Transportation.Application.PayoutLine.Commands;
using Transportation.Application.PayoutLine.Models;
using Transportation.Application.PayoutLine.Queries;
using Transportation.Mediator.Helper.Common.Models;

namespace Transportation.API.Controllers;

[ApiController]
[Route("api/[controller]/[action]")]
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
            request.DriverPayment,
            request.ExtraKmPayment,
            request.TaxiCompensation
        );

        return await _mediator.Send(command, cancellationToken);
    }

    [HttpDelete("{id:long}")]
    public async Task<bool> Delete(
        long id,
        CancellationToken cancellationToken = default)
    {
        return await _mediator.Send(new DeletePayoutLineCommand(id), cancellationToken);
    }
}

