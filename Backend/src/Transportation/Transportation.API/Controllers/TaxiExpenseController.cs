using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Transportation.API.Models;
using Transportation.Application.TaxiExpense.Commands;
using Transportation.Application.TaxiExpense.Models;
using Transportation.Application.TaxiExpense.Queries;
using Transportation.Domain.Entities;
using Transportation.Mediator.Helper.Common.Models;
using Transportation.Shared.Authorization;

namespace Transportation.API.Controllers;

public class TaxiExpenseController : BaseController
{
    public TaxiExpenseController(IMediator mediator) : base(mediator)
    {
    }

    [HttpGet]
    [RoleAuthorize(RoleNames.Admin, RoleNames.Accountant, RoleNames.RouteManager)]
    public async Task<PaginatedResult<TaxiExpenseDto>> GetAll(
        [FromQuery] GetAllTaxiExpense getAllTaxiExpense,
        CancellationToken cancellationToken = default)
    {
        return await _mediator.Send(getAllTaxiExpense, cancellationToken);
    }

    [HttpGet("{id:long}")]
    [RoleAuthorize(RoleNames.Admin, RoleNames.Accountant, RoleNames.RouteManager)]
    public async Task<TaxiExpenseDto> GetById(long id, CancellationToken cancellationToken = default)
    {
        return await _mediator.Send(new GetTaxiExpenseById(id), cancellationToken);
    }

    [HttpGet("by-status")]
    [RoleAuthorize(RoleNames.Admin, RoleNames.Accountant, RoleNames.RouteManager)]
    public async Task<PaginatedResult<TaxiExpenseDto>> GetByStatus(
        [FromQuery] GetTaxiExpensesByStatus requestedStatus,
        CancellationToken cancellationToken = default)
    {
        return await _mediator.Send(requestedStatus, cancellationToken);
    }

    [HttpPost]
    [RoleAuthorize(RoleNames.Admin, RoleNames.CrewLead, RoleNames.DriverLead)]
    public async Task<TaxiExpenseDto> Create([FromBody] CreateTaxiExpenseCommand command,
        CancellationToken cancellationToken = default)
    {
        return await _mediator.Send(command, cancellationToken);
    }

    [HttpPut("{id:long}")]
    [RoleAuthorize(RoleNames.Admin, RoleNames.CrewLead, RoleNames.DriverLead)]
    public async Task<NoContentResult> Update(long id,
        [FromQuery] UpdateTaxiExpenseRequest request,
        CancellationToken cancellationToken = default)
    {
        await _mediator.Send(
            new UpdateTaxiExpenseCommand(
                Id: id,
                PaidById: request.PaidById,
                Leg: request.Leg,
                Amount: request.Amount
            ), cancellationToken
        );

        return new NoContentResult();
    }

    [HttpDelete("{id:long}")]
    [RoleAuthorize(RoleNames.Admin, RoleNames.CrewLead, RoleNames.DriverLead)]
    public async Task<NoContentResult> Delete(long id, CancellationToken cancellationToken = default)
    {
        await _mediator.Send(new DeleteTaxiExpenseCommand(id), cancellationToken);

        return new NoContentResult();
    }

    [HttpPut("{id:long}/approve")]
    [RoleAuthorize(RoleNames.Admin, RoleNames.Accountant)]
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

    [HttpPut("{id:long}/reject")]
    [RoleAuthorize(RoleNames.Admin, RoleNames.Accountant)]
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

    [HttpPut("{id:long}/paid")]
    [RoleAuthorize(RoleNames.Admin, RoleNames.Accountant)]
    public async Task<NoContentResult> Paid(long id, CancellationToken cancellationToken = default)
    {
        await _mediator.Send(new TaxiExpensePaidCommand(TaxiExpenseId: id), cancellationToken);

        return new NoContentResult();
    }
}