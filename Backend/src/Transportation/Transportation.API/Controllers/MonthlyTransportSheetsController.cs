using MediatR;
using Microsoft.AspNetCore.Mvc;
using Transportation.Application.MonthlyTransportSheet.Commands;
using Transportation.Application.MonthlyTransportSheet.Models;
using Transportation.Application.MonthlyTransportSheet.Queries;
using Transportation.Mediator.Helper.Common.Models;
using Transportation.Shared.Authorization;

namespace Transportation.API.Controllers;

[RoleAuthorize(RoleNames.Accountant, RoleNames.Admin)]
public class MonthlyTransportSheetsController : BaseController
{
    public MonthlyTransportSheetsController(IMediator mediator) : base(mediator)
    {
    }

    [HttpGet]
    public async Task<PaginatedResult<MonthlyTransportSheetDto>> GetAll(
        [FromQuery] GetAllMonthlyTransportSheets query,
        CancellationToken cancellationToken = default)
    {
        return await _mediator.Send(query, cancellationToken);
    }

    [HttpGet("{id:long}")]
    public async Task<MonthlyTransportSheetDto> GetById(
        long id,
        CancellationToken cancellationToken = default)
    {
        return await _mediator.Send(new GetMonthlyTransportSheetById(id), cancellationToken);
    }

    [HttpPost]
    public async Task<MonthlyTransportSheetDto> GetPreview(
        [FromBody] PreviewMonthlyTransportSheetQuery query,
        CancellationToken cancellationToken = default)
    {
        return await _mediator.Send(query, cancellationToken);
    }

    [HttpPost]
    public async Task<MonthlyTransportSheetDto> Generate(
        [FromBody] GenerateMonthlyTransportSheetCommand command,
        CancellationToken cancellationToken = default)
    {
        return await _mediator.Send(command, cancellationToken);
    }

    [HttpPut("{id:long}")]
    public async Task<NoContentResult> Confirm(long id, CancellationToken cancellationToken = default)
    {
        await _mediator.Send(new ConfirmMonthlyTransportSheetCommand(id), cancellationToken);

        return new NoContentResult();
    }

    /// <summary>Puts a confirmed sheet back to draft so the month can be corrected.</summary>
    [HttpPut("{id:long}")]
    public async Task<NoContentResult> Unconfirm(long id, CancellationToken cancellationToken = default)
    {
        await _mediator.Send(new UnconfirmMonthlyTransportSheetCommand(id), cancellationToken);

        return new NoContentResult();
    }

    /// <summary>Releases the crew's month to its lead. Settlement is per sheet, once.</summary>
    [HttpPut("{id:long}")]
    public async Task<NoContentResult> MarkPaid(long id, CancellationToken cancellationToken = default)
    {
        await _mediator.Send(new MarkMonthlyTransportSheetPaidCommand(id), cancellationToken);

        return new NoContentResult();
    }

    [HttpDelete("{id:long}")]
    public async Task<NoContentResult> Delete(
        long id,
        CancellationToken cancellationToken = default)
    {
        await _mediator.Send(new DeleteMonthlyTransportSheetCommand(id), cancellationToken);

        return new NoContentResult();
    }
}
