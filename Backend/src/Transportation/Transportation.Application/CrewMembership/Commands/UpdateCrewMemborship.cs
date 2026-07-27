using FluentValidation;
using Transportation.Application.CrewMembership.Repositories;
using Transportation.Mediator.Helper.Exceptions;
using Transportation.Mediator.Helper.Persistence;
using Transportation.Application.CrewMembership.Models;
using Transportation.Application.CrewMembership.Specification;
using Transportation.Mediator.Helper.Commands;
using Transportation.Mediator.Helper.Common.Extensions;

namespace Transportation.Application.CrewMembership.Commands;

public sealed record UpdateCrewMembershipCommand(
    long Id,
    DateTime? ActiveFrom,
    DateTime? ActiveTo
) : ICommand<CrewMembershipDto>;

// ReSharper disable once UnusedType.Global
public class UpdateCrewMembershipCommandValidator : AbstractValidator<UpdateCrewMembershipCommand>
{
    public UpdateCrewMembershipCommandValidator()
    {
        RuleFor(c => c.Id)
            .GreaterThan(0).WithMessage("Id must be greater than 0");

        When(c => c.ActiveFrom.HasValue && c.ActiveTo.HasValue, () =>
        {
            RuleFor(c => c.ActiveFrom!.Value)
                .LessThan(c => c.ActiveTo!.Value).WithMessage("ActiveFrom must be earlier than ActiveTo.");
        });
    }
}

internal sealed class UpdateCrewMembershipCommandHandler : ICommandHandler<UpdateCrewMembershipCommand, CrewMembershipDto>
{
    private readonly ICrewMembershipRepository _crewMembershipRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly CrewMembershipMapper _mapper;
    private readonly TimeProvider _timeProvider;

    public UpdateCrewMembershipCommandHandler(
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

    public async Task<CrewMembershipDto> Handle(UpdateCrewMembershipCommand request, CancellationToken cancellationToken)
    {
        var spec = new CrewMembershipByIdSpec(request.Id);
        var entity = await _crewMembershipRepository.FirstOrDefaultAsync(spec, cancellationToken);

        if (entity is null)
            throw new ResourceNotFoundException(CrewMembershipErrors.NotFound);

        var effectiveFrom = request.ActiveFrom ?? entity.ActiveFrom;
        var effectiveTo = request.ActiveTo ?? entity.ActiveTo;

        var now = _timeProvider.GetLocalDateTimeNowKindUtc();

        if (request.ActiveFrom.HasValue && request.ActiveFrom.Value.Date < now.Date) 
            throw new BusinessLogicException(CrewMembershipErrors.ActiveFromInvalid);
        
        if (effectiveTo.HasValue && effectiveFrom >= effectiveTo.Value)
            throw new BusinessLogicException(CrewMembershipErrors.ActiveToInvalid);
        
        if (request.ActiveFrom.HasValue)
            entity.ActiveFrom = request.ActiveFrom.Value;

        if (request.ActiveTo.HasValue)
            entity.ActiveTo = request.ActiveTo.Value;
        
        entity.IsActive = now >= entity.ActiveFrom && (!entity.ActiveTo.HasValue || entity.ActiveTo.Value > now);
        entity.UpdatedAt = now;

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return _mapper.Map(entity);
    }
}