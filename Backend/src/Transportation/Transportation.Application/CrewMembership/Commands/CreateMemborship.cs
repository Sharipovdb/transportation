using Ardalis.Specification;
using FluentValidation;
using Transportation.Application.Crew;
using Transportation.Application.Crew.Repositories;
using Transportation.Application.Crew.Specification;
using Transportation.Application.CrewMembership.Repositories;
using Transportation.Mediator.Helper.Exceptions;
using Transportation.Mediator.Helper.Persistence;
using Transportation.Application.CrewMembership.Models;
using Transportation.Application.CrewMembership.Specification;
using Transportation.Mediator.Helper.Commands;
using Transportation.Mediator.Helper.Common.Extensions;

namespace Transportation.Application.CrewMembership.Commands;

public sealed record CreateCrewMembershipCommand(
    long CrewId,
    long UserId
) : ICommand<CrewMembershipDto>;

// ReSharper disable once UnusedType.Global
public class CreateCrewMembershipCommandValidator : AbstractValidator<CreateCrewMembershipCommand>
{
    public CreateCrewMembershipCommandValidator()
    {
        RuleFor(x => x.CrewId)
            .GreaterThan(0)
            .WithMessage("CrewId must be greater than 0");

        RuleFor(x => x.UserId)
            .GreaterThan(0)
            .WithMessage("Employee ID must be greater than 0");
    }
}

internal sealed class CreateMembershipCommandHandler : ICommandHandler<CreateCrewMembershipCommand, CrewMembershipDto>
{
    private readonly ICrewMembershipRepository _crewMembershipRepository;
    private readonly ICrewRepository _crewRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly CrewMembershipMapper _mapper;
    private readonly TimeProvider _timeProvider;

    public CreateMembershipCommandHandler(
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

    public async Task<CrewMembershipDto> Handle(
        CreateCrewMembershipCommand request,
        CancellationToken cancellationToken)
    {
        var crewSpec = new CrewByIdSpec(request.CrewId);
        var crew = await _crewRepository.FirstOrDefaultAsync(crewSpec, cancellationToken);

        if (crew is null)
            throw new ResourceNotFoundException(CrewErrors.NotFound);
        
        var existInCurrentCrewSpec = new CrewMembershipByCrewIdAndUserIdSpec(request.CrewId, request.UserId);

        if (await _crewMembershipRepository.AnyAsync(existInCurrentCrewSpec, cancellationToken))
            throw new BusinessLogicException(CrewMembershipErrors.AlreadyExists);

        var existInAnyCrewSpec = new ActiveCrewMembershipByUserIdSpec(request.UserId);

        if (await _crewMembershipRepository.AnyAsync(existInAnyCrewSpec, cancellationToken))
            throw new BusinessLogicException(CrewMembershipErrors.EmployeerIsAsigned);

        var specCm = new DbSpecification<Domain.Entities.CrewMembership>();
        specCm.Query.Where(x => x.CrewId == request.CrewId && x.IsActive && !x.IsDeleted);

        var count = await _crewMembershipRepository.CountAsync(specCm, cancellationToken);

        if (count >= crew.SeatCapacity)
            throw new BusinessLogicException(CrewMembershipErrors.FullSeatCapacity);
        
        var entity = new Domain.Entities.CrewMembership()
        {
            CrewId = request.CrewId,
            UserId = request.UserId,
            ActiveFrom = _timeProvider.GetLocalDateTimeNowKindUtc(),
            IsActive = true,
            UpdatedAt = _timeProvider.GetLocalDateTimeNowKindUtc()
        };

        await _crewMembershipRepository.AddAsync(entity, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return _mapper.Map(entity);
    }
}