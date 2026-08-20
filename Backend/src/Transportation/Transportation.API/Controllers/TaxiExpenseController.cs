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

    // Same scope as the days behind them: a lead sees their own crews' fares, and only
    // the accountant rules on any of them.
    [HttpGet]
    [RoleAuthorize(RoleNames.Admin, RoleNames.Accountant, RoleNames.RouteManager, RoleNames.CrewLead, RoleNames.DriverLead)]
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

    // A taxi ride is recorded on the transport day it belongs to, never here: that is what
    // keeps a taxi leg from existing without a fare behind it, and a fare from existing
    // without a ride. This screen only corrects, approves or rejects what the day logged.
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
                Amount: request.Amount
            ), cancellationToken
        );

        return new NoContentResult();
    }

    [HttpPut("{id:long}")]
    [RoleAuthorize(RoleNames.Admin, RoleNames.Accountant)]
    public Task<NoContentResult> Approve(long id, CancellationToken cancellationToken = default)
    {
        return SetStatus(id, TaxiExpenseStatus.Approved, cancellationToken);
    }

    [HttpPut("{id:long}")]
    [RoleAuthorize(RoleNames.Admin, RoleNames.Accountant)]
    public Task<NoContentResult> Reject(long id, CancellationToken cancellationToken = default)
    {
        return SetStatus(id, TaxiExpenseStatus.Rejected, cancellationToken);
    }

    // There is no "mark this fare paid" action: money is released a whole crew-month at
    // a time by confirming its monthly sheet, which is what moves its fares on to Paid.
    private async Task<NoContentResult> SetStatus(long id, TaxiExpenseStatus status,
        CancellationToken cancellationToken)
    {
        await _mediator.Send(
            new UpdateTaxiExpenseReimbursementStatusCommand(
                TaxiExpenseId: id,
                TaxiExpenseStatus: status
            ), cancellationToken
        );

        return new NoContentResult();
    }
}