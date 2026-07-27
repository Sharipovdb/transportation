using MediatR;
using Microsoft.AspNetCore.Mvc;
using Transportation.Application.AbsenceNotice.Commands;
using Transportation.Application.AbsenceNotice.Models;
using Transportation.Application.AbsenceNotice.Queries;
using Transportation.Mediator.Helper.Common.Models;

namespace Transportation.API.Controllers;

public class AbsenceNoticesController : BaseController
{
    public AbsenceNoticesController(IMediator mediator) : base(mediator)
    {
    }

    [HttpGet]
    public Task<PaginatedResult<AbsenceNoticeDto>> GetAll(
        [FromQuery] GetAllAbsenceNotices query,
        CancellationToken cancellationToken)
    {
        return _mediator.Send(query, cancellationToken);
    }

    [HttpGet("{id:long}")]
    public async Task<AbsenceNoticeDto> GetById(long id, CancellationToken cancellationToken)
    {
        return await _mediator.Send(new GetAbsenceNoticeById(id), cancellationToken);
    }

    [HttpPost]
    public async Task<AbsenceNoticeDto> Create(
        [FromBody] CreateAbsenceNoticeCommand command,
        CancellationToken cancellationToken)
    {
        return await _mediator.Send(command, cancellationToken);
    }

    [HttpPut("{id:long}")]
    public async Task<AbsenceNoticeDto> Update(
        long id,
        [FromBody] UpdateAbsenceNoticeCommand command,
        CancellationToken cancellationToken)
    {
        return await _mediator.Send(command with { AbsenceNoticeId = id }, cancellationToken);
    }

    [HttpDelete("{id:long}")]
    public async Task<IActionResult> Delete(long id, CancellationToken cancellationToken)
    {
        await _mediator.Send(new DeleteAbsenceNoticeCommand(id), cancellationToken);
        return NoContent();
    }
}