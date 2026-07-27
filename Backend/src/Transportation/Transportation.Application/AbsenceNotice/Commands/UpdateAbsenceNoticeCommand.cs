using Transportation.Application.AbsenceNotice.Models;
using Transportation.Application.AbsenceNotice.Repositories;
using Transportation.Application.AbsenceNotice.Specifications;
using Transportation.Domain.Entities;
using Transportation.Mediator.Helper.Commands;
using Transportation.Mediator.Helper.Common.Extensions;
using Transportation.Mediator.Helper.Exceptions;
using Transportation.Mediator.Helper.Persistence;

namespace Transportation.Application.AbsenceNotice.Commands;

public sealed record UpdateAbsenceNoticeCommand(
    long AbsenceNoticeId,
    DateOnly? Date,
    AbsenceScope? Scope,
    AbsenceType? Type,
    string? Reason
) : ICommand<AbsenceNoticeDto>;

internal sealed class UpdateAbsenceNoticeHandler : ICommandHandler<UpdateAbsenceNoticeCommand, AbsenceNoticeDto>
{
    private readonly IAbsenceNoticeRepository _absenceNoticeRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly AbsenceNoticeMapper _mapper;
    private readonly TimeProvider _timeProvider;

    public UpdateAbsenceNoticeHandler(
        IAbsenceNoticeRepository absenceNoticeRepository,
        IUnitOfWork unitOfWork,
        AbsenceNoticeMapper mapper,
        TimeProvider timeProvider)
    {
        _absenceNoticeRepository = absenceNoticeRepository;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _timeProvider = timeProvider;
    }

    public async Task<AbsenceNoticeDto> Handle(
        UpdateAbsenceNoticeCommand request,
        CancellationToken cancellationToken)
    {
        var spec = new AbsenceNoticeByIdSpec(request.AbsenceNoticeId);
        var entity = await _absenceNoticeRepository.FirstOrDefaultAsync(spec, cancellationToken);

        if (entity is null)
            throw new BusinessLogicException(AbsenceNoticeErrors.NotFound);

        if (request.Reason is not null)
            entity.Reason = request.Reason;

        if (request.Scope is not null)
            entity.Scope = (AbsenceScope)request.Scope;

        if (request.Type is not null)
            entity.Type = (AbsenceType)request.Type;

        if (request.Date.HasValue)
            entity.Date = (DateOnly)request.Date;

        entity.UpdatedAt = _timeProvider.GetLocalDateTimeNowKindUtc();

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return _mapper.Map(entity);
    }
}