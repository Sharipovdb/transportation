using FluentValidation;
using Transportation.Application.Crew;
using Transportation.Application.Crew.Repositories;
using Transportation.Application.Crew.Specification;
using Transportation.Application.CrewMembership.Models;
using Transportation.Application.CrewMembership.Repositories;
using Transportation.Application.CrewMembership.Specification;
using Transportation.Mediator.Helper.Commands;
using Transportation.Mediator.Helper.Common.Extensions;
using Transportation.Mediator.Helper.Exceptions;
using Transportation.Mediator.Helper.Persistence;

namespace Transportation.Application.CrewMembership.Commands;

public sealed record CreateCrewMembershipCommand(
    long CrewId,
    List<long> UserIds
) : ICommand<List<CrewMembershipDto>>;

// ReSharper disable once UnusedType.Global
public class CreateCrewMembershipCommandValidator : AbstractValidator<CreateCrewMembershipCommand>
{
    public CreateCrewMembershipCommandValidator()
    {
        RuleFor(x => x.CrewId)
            .GreaterThan(0)
            .WithMessage("CrewId must be greater than 0");
    }
}

internal sealed class
    CreateCrewMembershipCommandHandler : ICommandHandler<CreateCrewMembershipCommand, List<CrewMembershipDto>>
{
    private readonly ICrewMembershipRepository _crewMembershipRepository;
    private readonly ICrewRepository _crewRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly CrewMembershipMapper _mapper;
    private readonly TimeProvider _timeProvider;

    public CreateCrewMembershipCommandHandler(
        ICrewMembershipRepository crewMembershipRepository,
        IUnitOfWork unitOfWork,
        CrewMembershipMapper mapper,
        ICrewRepository crewRepository,
        TimeProvider timeProvider)
    {
        _crewMembershipRepository = crewMembershipRepository;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _crewRepository = crewRepository;
        _timeProvider = timeProvider;
    }

    public async Task<List<CrewMembershipDto>> Handle(CreateCrewMembershipCommand request,
        CancellationToken cancellationToken)
    {
        var crew = await _crewRepository
            .FirstOrDefaultAsync(new CrewByIdSpec(request.CrewId), cancellationToken);

        if (crew is null)
            throw new ResourceNotFoundException(CrewErrors.NotFound);

        var activeMemberships = await _crewMembershipRepository.AnyAsync(
            new ActiveCrewMembershipByUserIdSpec(request.CrewId, request.UserIds, asNoTracking: true),
            cancellationToken);

        if (activeMemberships)
            throw new BusinessLogicException(CrewMembershipErrors.EmployeeIsAssigned);

        var activeMembersCount = await _crewMembershipRepository.CountAsync(
            new ActiveCrewMembershipsByCrewIdSpec(request.CrewId, asNoTracking: true), cancellationToken);

        if (activeMembersCount + request.UserIds.Count > crew.SeatCapacity)
            throw new BusinessLogicException(CrewMembershipErrors.FullSeatCapacity);

        var now = _timeProvider.GetLocalDateTimeNowKindUtc();
        List<Domain.Entities.CrewMembership> crewMemberships = new();

        foreach (var userId in request.UserIds)
        {
            crewMemberships.Add(new Domain.Entities.CrewMembership
            {
                CrewId = request.CrewId,
                UserId = userId,
                ActiveFrom = now,
                IsActive = true,
                CreatedAt = now,
                UpdatedAt = now
            });
        }

        await _crewMembershipRepository.AddRangeAsync(crewMemberships, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return _mapper.Map(crewMemberships);
    }
}