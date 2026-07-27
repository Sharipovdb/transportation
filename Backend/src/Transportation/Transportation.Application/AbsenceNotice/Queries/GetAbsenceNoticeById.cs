using Transportation.Application.AbsenceNotice.Models;
using Transportation.Application.AbsenceNotice.Repositories;
using Transportation.Application.AbsenceNotice.Specifications;
using Transportation.Mediator.Helper.Exceptions;
using Transportation.Mediator.Helper.Queries;

namespace Transportation.Application.AbsenceNotice.Queries;

public sealed record GetAbsenceNoticeById(long AbsenceNoticeId) : IQuery<AbsenceNoticeDto>;
    
internal sealed class GetAbsenceNoticeByIdHandler : IQueryHandler<GetAbsenceNoticeById, AbsenceNoticeDto>
{
    private readonly IAbsenceNoticeRepository _absenceNoticeRepository;
    private readonly AbsenceNoticeMapper _mapper;

    public GetAbsenceNoticeByIdHandler(
        IAbsenceNoticeRepository absenceNoticeRepository,
        AbsenceNoticeMapper mapper)
    {
        _absenceNoticeRepository = absenceNoticeRepository;
        _mapper = mapper;
    }

    public async Task<AbsenceNoticeDto> Handle(
        GetAbsenceNoticeById request, 
        CancellationToken cancellationToken)
    {
        var spec = new AbsenceNoticeByIdSpec(request.AbsenceNoticeId);
        var entity = await _absenceNoticeRepository.FirstOrDefaultAsync(spec, cancellationToken);

        if (entity is null)
            throw new BusinessLogicException(AbsenceNoticeErrors.NotFound);

        return _mapper.Map(entity);
    }
}