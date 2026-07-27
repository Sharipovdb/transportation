using Ardalis.Specification;
using Transportation.Mediator.Helper.Commands;
using Transportation.Mediator.Helper.Persistence;
using FluentValidation;
using Transportation.Application.CrewMembership.Repositories;
using Transportation.Application.CrewMembership.Specification;
using Transportation.Application.Crew.Repositories;
using Transportation.Application.Crew;
using Transportation.Application.Crew.Specification;
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
        RuleFor(command => command.UserId)
            .GreaterThan(0)
            .WithMessage("The specified user id must be greater than 0");

        RuleFor(c => c.OldCrewId)
            .GreaterThan(0)
            .WithMessage("The old crew id must be greater than 0");

        RuleFor(command => command.NewCrewId)
            .GreaterThan(0)
            .WithMessage("The new crew id must be greater than 0");
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
        var crewSpec = new CrewByIdSpec(request.NewCrewId);
        var crewExists = await _crewRepository.FirstOrDefaultAsync(crewSpec, cancellationToken);
        
        if (crewExists is null)
            throw new ResourceNotFoundException(CrewErrors.NotFound);
        
        var crewMemberSpec = new CrewMembershipByCrewIdAndUserIdSpec(request.OldCrewId, request.UserId);
        var entity = await _crewMembershipRepository.FirstOrDefaultAsync(crewMemberSpec, cancellationToken);

        if (entity is null)
            throw new ResourceNotFoundException(CrewMembershipErrors.NotFound);

        var specCm = new DbSpecification<Domain.Entities.CrewMembership>();
        specCm.Query.Where(x => x.CrewId == crewExists.Id);

        var count = await _crewMembershipRepository.CountAsync(specCm, cancellationToken);

        if (count >= crewExists.SeatCapacity)
            throw new BusinessLogicException(CrewMembershipErrors.FullSeatCapacity);

        entity.ActiveTo = _timeProvider.GetLocalDateTimeNowKindUtc();
        entity.IsActive = false;
        entity.UpdatedAt = _timeProvider.GetLocalDateTimeNowKindUtc();

        var newMembership = new Domain.Entities.CrewMembership
        {
            CrewId = request.NewCrewId,
            UserId = request.UserId,
            IsActive = true,
            ActiveFrom = _timeProvider.GetLocalDateTimeNowKindUtc(),
            CreatedAt = _timeProvider.GetLocalDateTimeNowKindUtc(),
        };

        await _crewMembershipRepository.AddAsync(newMembership, cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return _crewMembershipMapper.Map(newMembership);
    }
}