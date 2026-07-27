using Transportation.Application.AbsenceNotice.Repositories;
using Transportation.Application.AbsenceNotice.Specifications;
using Transportation.Mediator.Helper.Commands;
using Transportation.Mediator.Helper.Exceptions;
using Transportation.Mediator.Helper.Persistence;

namespace Transportation.Application.AbsenceNotice.Commands;

public sealed record DeleteAbsenceNoticeCommand(long AbsenceNoticeId) : ICommand<bool>;

internal sealed class DeleteAbsenceNoticeHandler : ICommandHandler<DeleteAbsenceNoticeCommand, bool>
{
    private readonly IAbsenceNoticeRepository _absenceNoticeRepository;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteAbsenceNoticeHandler(
        IAbsenceNoticeRepository absenceNoticeRepository,
        IUnitOfWork unitOfWork)
    {
        _absenceNoticeRepository = absenceNoticeRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<bool> Handle(DeleteAbsenceNoticeCommand request, CancellationToken cancellationToken)
    {
        var spec = new AbsenceNoticeByIdSpec(request.AbsenceNoticeId);
        var entity = await _absenceNoticeRepository.FirstOrDefaultAsync(spec, cancellationToken);

        if (entity is null)
            throw new BusinessLogicException(AbsenceNoticeErrors.NotFound);

        entity.IsDeleted = true;

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return true;
    }
}