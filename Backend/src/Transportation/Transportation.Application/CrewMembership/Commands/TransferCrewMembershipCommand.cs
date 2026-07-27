using Transportation.Mediator.Helper.Commands;
using Transportation.Mediator.Helper.Persistence;
using FluentValidation;
using Transportation.Application.CrewMembership.Repositories;
using Transportation.Application.CrewMembership.Specification;
using Transportation.Application.Crew.Repositories;
using Transportation.Application.Crew;
using Transportation.Application.CrewMembership.Models;
using Transportation.Mediator.Helper.Common.Extensions;
using Transportation.Mediator.Helper.Exceptions;

namespace Transportation.Application.CrewMembership.Commands;

public record TransferCrewMembershipCommand(
    long UserId,
    long OldCrewId,
    long NewCrewId
) : ICommand<CrewMembershipDto>;

// ReSharper disable once UnusedType.Global
public class TransferCrewMembershipCommandValidator : AbstractValidator<TransferCrewMembershipCommand>
{
    public TransferCrewMembershipCommandValidator()
    {
        RuleFor(x => x.UserId)
            .GreaterThan(0).WithMessage("The specified user id must be greater than 0");

        RuleFor(x => x.OldCrewId)
            .GreaterThan(0).WithMessage("The old crew id must be greater than 0");

        RuleFor(x => x.NewCrewId)
            .GreaterThan(0).WithMessage("The new crew id must be greater than 0")
            .NotEqual(x => x.OldCrewId).WithMessage("New crew must be different from the old crew");
    }
}

public sealed class TransferCrewMembershipCommandHandler : ICommandHandler<TransferCrewMembershipCommand, CrewMembershipDto>
{
    private readonly ICrewMembershipRepository _crewMembershipRepository;
    private readonly ICrewRepository _crewRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly TimeProvider _timeProvider;
    private readonly CrewMembershipMapper _crewMembershipMapper;

    public TransferCrewMembershipCommandHandler(
        IUnitOfWork unitOfWork,
        ICrewMembershipRepository crewMembershipRepository,
        ICrewRepository crewRepository, 
        TimeProvider timeProvider, 
        CrewMembershipMapper crewMembershipMapper)
    {
        _unitOfWork = unitOfWork;
        _crewMembershipRepository = crewMembershipRepository;
        _crewRepository = crewRepository;
        _timeProvider = timeProvider;
        _crewMembershipMapper = crewMembershipMapper;
    }

    public async Task<CrewMembershipDto> Handle(TransferCrewMembershipCommand request, CancellationToken cancellationToken)
    {
        var newCrew = await _crewRepository.GetByIdAsync(request.NewCrewId, cancellationToken);

        if (newCrew is null)
            throw new ResourceNotFoundException(CrewErrors.NotFound);

        var crewMemberSpec = new CrewMembershipByCrewIdAndUserIdSpec(request.OldCrewId, request.UserId);
        var oldMembership = await _crewMembershipRepository.FirstOrDefaultAsync(crewMemberSpec, cancellationToken);

        if (oldMembership is null)
            throw new ResourceNotFoundException(CrewMembershipErrors.NotFound);

        var activeMembersInNewCrew = await _crewMembershipRepository.CountAsync(
            new ActiveCrewMembershipsByCrewIdSpec(request.NewCrewId, asNoTracking: true), cancellationToken);

        if (activeMembersInNewCrew >= newCrew.SeatCapacity)
            throw new BusinessLogicException(CrewMembershipErrors.FullSeatCapacity);

        var now = _timeProvider.GetLocalDateTimeNowKindUtc();

        oldMembership.ActiveTo = now;
        oldMembership.IsActive = false;
        oldMembership.UpdatedAt = now;

        var newMembership = new Domain.Entities.CrewMembership
        {
            CrewId = request.NewCrewId,
            UserId = request.UserId,
            IsActive = true,
            ActiveFrom = now,
            CreatedAt = now,
            UpdatedAt = now
        };

        await _crewMembershipRepository.AddAsync(newMembership, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return _crewMembershipMapper.Map(newMembership);
    }
}