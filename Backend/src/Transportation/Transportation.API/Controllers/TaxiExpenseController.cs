using MediatR;
using Microsoft.AspNetCore.Mvc;
using Transportation.API.Models;
using Transportation.Application.TaxiExpense.Commands;
using Transportation.Application.TaxiExpense.Models;
using Transportation.Application.TaxiExpense.Queries;
using Transportation.Domain.Entities;
using Transportation.Mediator.Helper.Common.Models;

namespace Transportation.API.Controllers;

public class TaxiExpenseController : BaseController
{
    public TaxiExpenseController(IMediator mediator) : base(mediator)
    {
    }

    [HttpGet]
    public async Task<PaginatedResult<TaxiExpenseDto>> GetAll(
        [FromQuery] GetAllTaxiExpense getAllTaxiExpense,
        CancellationToken cancellationToken = default)
    {
        return await _mediator.Send(getAllTaxiExpense, cancellationToken);
    }

    [HttpGet("{id:long}")]
    public async Task<TaxiExpenseDto> GetByIdAsync(long id, CancellationToken cancellationToken = default)
    {
        return await _mediator.Send(new GetTaxiExpenseById(id), cancellationToken);
    }

    [HttpGet]
    public async Task<PaginatedResult<TaxiExpenseDto>> GetByStatus(
        [FromQuery] GetTaxiExpensesByStatus requestedStatus,
        CancellationToken cancellationToken = default)
    {
        return await _mediator.Send(requestedStatus, cancellationToken);
    }

    [HttpPost]
    public async Task<TaxiExpenseDto> CreateAsync([FromBody] CreateTaxiExpenseCommand command,
        CancellationToken cancellationToken = default)
    {
        return await _mediator.Send(command, cancellationToken);
    }

    [HttpPut("{id:long}")]
    public async Task<NoContentResult> Update(long id,
        [FromQuery] UpdateTaxiExpenseRequest request,
        CancellationToken cancellationToken = default)
    {
        await _mediator.Send(
            new UpdateTaxiExpenseCommand(
                Id: id,
                TransportDayId: request.TransportDayId,
                PaidById: request.PaidById,
                Leg: request.Leg,
                Amount: request.Amount,
                TaxiExpenseStatus: request.TaxiExpenseStatus
            ), cancellationToken
        );

        return new NoContentResult();
    }

    [HttpDelete("{id:long}")]
    public async Task<NoContentResult> Delete(long id, CancellationToken cancellationToken = default)
    {
        await _mediator.Send(new DeleteTaxiExpenseCommand(id), cancellationToken);

        return new NoContentResult();
    }

    [HttpPut("{id:long}")]
    public async Task<NoContentResult> Approve(long id, CancellationToken cancellationToken = default)
    {
        await _mediator.Send(
            new UpdateTaxiExpenseReimbursementStatusCommand(
                TaxiExpenseId: id,
                TaxiExpenseStatus: TaxiExpenseStatus.Approved
            ), cancellationToken
        );

        return new NoContentResult();
    }

    [HttpPut("{id:long}")]
    public async Task<NoContentResult> Reject(long id, CancellationToken cancellationToken = default)
    {
        await _mediator.Send(
            new UpdateTaxiExpenseReimbursementStatusCommand(
                TaxiExpenseId: id,
                TaxiExpenseStatus: TaxiExpenseStatus.Rejected
            ), cancellationToken
        );

        return new NoContentResult();
    }

    [HttpPut("{id:long}")]
    public async Task<NoContentResult> Paid(long id, CancellationToken cancellationToken = default)
    {
        await _mediator.Send(new TaxiExpensePaidCommand(TaxiExpenseId: id), cancellationToken);

        return new NoContentResult();
    }
}