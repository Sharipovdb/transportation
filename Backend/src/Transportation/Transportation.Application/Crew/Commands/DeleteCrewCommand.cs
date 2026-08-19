using FluentValidation;
using Transportation.Application.Crew.Repositories;
using Transportation.Application.Crew.Specification;
using Transportation.Application.CrewMembership.Repositories;
using Transportation.Application.CrewMembership.Specification;
using Transportation.Mediator.Helper.Commands;
using Transportation.Mediator.Helper.Common.Extensions;
using Transportation.Mediator.Helper.Exceptions;
using Transportation.Mediator.Helper.Persistence;

namespace Transportation.Application.Crew.Commands;

public record DeleteCrewCommand(long Id) : ICommand<bool>;

// ReSharper disable once UnusedType.Global
public class DeleteCrewCommandValidator : AbstractValidator<DeleteCrewCommand>
{
    public DeleteCrewCommandValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("Crew ID must be greater than 0");
    }
}

internal sealed class DeleteCrewCommandHandler : ICommandHandler<DeleteCrewCommand, bool>
{
    private readonly ICrewRepository _crewRepository;
    private readonly ICrewMembershipRepository _crewMembershipRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly TimeProvider _timeProvider;

    public DeleteCrewCommandHandler(
        ICrewRepository crewRepository,
        ICrewMembershipRepository crewMembershipRepository,
        IUnitOfWork unitOfWork,
        TimeProvider timeProvider)
    {
        _crewRepository = crewRepository;
        _crewMembershipRepository = crewMembershipRepository;
        _unitOfWork = unitOfWork;
        _timeProvider = timeProvider;
    }

    public async Task<bool> Handle(DeleteCrewCommand request, CancellationToken cancellationToken)
    {
        // Through the spec, not GetByIdAsync: an already-deleted crew must read as gone
        // rather than be "deleted" a second time.
        var entity = await _crewRepository
            .FirstOrDefaultAsync(new CrewByIdSpec(request.Id), cancellationToken);

        if (entity is null)
            throw new ResourceNotFoundException(CrewErrors.NotFound);

        var now = _timeProvider.GetLocalDateTimeNowKindUtc();

        entity.IsDeleted = true;
        entity.UpdatedAt = now;

        // Memberships only exist as part of the crew that holds them. Left behind, they
        // stay flagged active and keep counting people towards a crew that is gone.
        var memberships = await _crewMembershipRepository
            .ListAsync(new CrewMembershipByCrewIdSpec(request.Id), cancellationToken);

        foreach (var membership in memberships)
        {
            membership.IsDeleted = true;
            membership.IsActive = false;
            membership.ActiveTo ??= now;
            membership.UpdatedAt = now;
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return true;
    }
}
