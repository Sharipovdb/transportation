using FluentValidation;
using Transportation.Application.Common.Interfaces;
using Transportation.Application.Crew.Repositories;
using Transportation.Application.Crew.Specification;
using Transportation.Application.CrewMembership.Repositories;
using Transportation.Application.CrewMembership.Specification;
using Transportation.Mediator.Helper.Commands;
using Transportation.Mediator.Helper.Common.Extensions;
using Transportation.Mediator.Helper.Exceptions;
using Transportation.Mediator.Helper.Persistence;
using Transportation.Shared.Authorization;

namespace Transportation.Application.Crew.Commands;

public record DeleteCrewCommand(long Id) : ICommand<bool>;

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
    private readonly IRoleService _roleService; 

    public DeleteCrewCommandHandler(
        ICrewRepository crewRepository,
        ICrewMembershipRepository crewMembershipRepository,
        IUnitOfWork unitOfWork,
        TimeProvider timeProvider,
        IRoleService roleService) 
    {
        _crewRepository = crewRepository;
        _crewMembershipRepository = crewMembershipRepository;
        _unitOfWork = unitOfWork;
        _timeProvider = timeProvider;
        _roleService = roleService; 
    }

    public async Task<bool> Handle(DeleteCrewCommand request, CancellationToken cancellationToken)
    {
        var entity = await _crewRepository
            .FirstOrDefaultAsync(new CrewByIdSpec(request.Id), cancellationToken);

        if (entity is null)
            throw new ResourceNotFoundException(CrewErrors.NotFound);

        var now = _timeProvider.GetLocalDateTimeNowKindUtc();

        entity.IsDeleted = true;
        entity.UpdatedAt = now;

        if (entity.DriverLeadId.HasValue)
            await _roleService.RemoveAsync(RoleNames.DriverLead, entity.DriverLeadId.Value);
        if (entity.CrewLeadId.HasValue)
            await _roleService.RemoveAsync(RoleNames.CrewLead, entity.CrewLeadId.Value);

        entity.DriverLeadId = null;
        entity.CrewLeadId = null;

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