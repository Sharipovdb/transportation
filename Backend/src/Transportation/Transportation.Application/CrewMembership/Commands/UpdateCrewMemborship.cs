using FluentValidation;
using Transportation.Application.CrewMembership.Repositories;
using Transportation.Mediator.Helper.Exceptions;
using Transportation.Mediator.Helper.Persistence;
using Transportation.Application.CrewMembership.Models;
using Transportation.Application.CrewMembership.Specification;
using Transportation.Mediator.Helper.Commands;

namespace Transportation.Application.CrewMembership.Commands;

public sealed record UpdateCrewMembership(
    long Id,
    DateTime? ActiveFrom,
    DateTime? ActiveTo
) : ICommand<CrewMembershipDto>;

// ReSharper disable once UnusedType.Global
public class UpdateCrewMembershipValidator : AbstractValidator<UpdateCrewMembership>
{
    public UpdateCrewMembershipValidator()
    {
        RuleFor(c => c.Id)
            .GreaterThan(0)
            .WithMessage("Id must be greater than 0");

        RuleFor(c => c)
            .Must(c => c.ActiveFrom < c.ActiveTo)
            .When(c => c.ActiveFrom.HasValue && c.ActiveTo.HasValue)
            .WithMessage("ActiveFrom must be earlier than ActiveTo.");
    }
}

internal sealed class UpdateCrewMembershipHandler : ICommandHandler<UpdateCrewMembership, CrewMembershipDto>
{
    private readonly ICrewMembershipRepository _crewMembershipRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly CrewMembershipMapper _mapper;
    private readonly TimeProvider _timeProvider;

    private static readonly DateTime MinActiveFromDate = new(2026, 07, 30, 0, 0, 0, DateTimeKind.Utc);

    public UpdateCrewMembershipHandler(
        ICrewMembershipRepository crewMembershipRepository,
        IUnitOfWork unitOfWork,
        CrewMembershipMapper mapper, 
        TimeProvider timeProvider)
    {
        _crewMembershipRepository = crewMembershipRepository;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _timeProvider = timeProvider;
    }

    public async Task<CrewMembershipDto> Handle(UpdateCrewMembership request, CancellationToken cancellationToken)
    {
        var spec = new CrewMembershipByIdSpec(request.Id);
        var entity = await _crewMembershipRepository.FirstOrDefaultAsync(spec, cancellationToken);

        if (entity is null)
            throw new ResourceNotFoundException(CrewMembershipErrors.NotFound);

        var effectiveFrom = request.ActiveFrom ?? entity.ActiveFrom;
        var effectiveTo = request.ActiveTo ?? entity.ActiveTo;

        if (effectiveFrom < MinActiveFromDate)
            throw new BusinessLogicException(CrewMembershipErrors.ActiveFromUnderLine);
        
        if (effectiveTo.HasValue && effectiveFrom >= effectiveTo.Value)
            throw new BusinessLogicException(CrewMembershipErrors.ActiveToUnderLine);
        
        if (request.ActiveFrom.HasValue)
            entity.ActiveFrom = request.ActiveFrom.Value;

        if (request.ActiveTo.HasValue)
            entity.ActiveTo = request.ActiveTo.Value;
        
        var utcNow = _timeProvider.GetUtcNow().UtcDateTime;
        
        entity.IsActive = utcNow >= entity.ActiveFrom && (!entity.ActiveTo.HasValue || entity.ActiveTo.Value > utcNow);
    
        entity.UpdatedAt = utcNow;

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return _mapper.Map(entity);
    }
}