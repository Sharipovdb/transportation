using Ardalis.Specification;
using Transportation.Application.AbsenceNotice.Models;
using Transportation.Application.AbsenceNotice.Repositories;
using Transportation.Mediator.Helper.Common.Extensions;
using Transportation.Mediator.Helper.Common.Models;
using Transportation.Mediator.Helper.Persistence;
using Transportation.Mediator.Helper.Queries;

namespace Transportation.Application.AbsenceNotice.Queries;

public sealed record GetAllAbsenceNotices(
    long? UserId,
    DateOnly? Date,
    PaginationInfo PaginationInfo
) : IQuery<PaginatedResult<AbsenceNoticeDto>>;

internal sealed class GetAllAbsenceNoticesHandler
    : IQueryHandler<GetAllAbsenceNotices, PaginatedResult<AbsenceNoticeDto>>
{
    private readonly IAbsenceNoticeRepository _absenceNoticeRepository;
    private readonly AbsenceNoticeMapper _mapper;

    public GetAllAbsenceNoticesHandler(
        IAbsenceNoticeRepository absenceNoticeRepository,
        AbsenceNoticeMapper mapper)
    {
        _absenceNoticeRepository = absenceNoticeRepository;
        _mapper = mapper;
    }

    public async Task<PaginatedResult<AbsenceNoticeDto>> Handle(
        GetAllAbsenceNotices request,
        CancellationToken cancellationToken)
    {
        var spec = new ReadOnlySpecification<Domain.Entities.AbsenceNotice>();

        if (request.UserId.HasValue)
            spec.Query.Where(x => x.UserId == request.UserId);

        if (request.Date.HasValue)
            spec.Query.Where(x => x.Date == request.Date);

        spec.Query
            .OrderByDescending(x => x.Date)
            .Where(x => !x.IsDeleted)
            .WithPagination(request.PaginationInfo);

        var items = await _absenceNoticeRepository.ListAsync(spec, cancellationToken);
        var count = await _absenceNoticeRepository.CountAsync(spec, cancellationToken);

        var mappedAbsenceNotices = _mapper.Map(items);

        return new PaginatedResult<AbsenceNoticeDto>(mappedAbsenceNotices, count);
    }
}