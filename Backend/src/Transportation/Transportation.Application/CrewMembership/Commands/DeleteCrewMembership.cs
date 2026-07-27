using FluentValidation;
using Transportation.Application.CrewMembership.Models;
using Transportation.Application.CrewMembership.Repositories;
using Transportation.Mediator.Helper.Commands;
using Transportation.Mediator.Helper.Common.Extensions;
using Transportation.Mediator.Helper.Exceptions;
using Transportation.Mediator.Helper.Persistence;

namespace Transportation.Application.CrewMembership.Commands;

public sealed record DeleteCrewMembershipCommand(long Id) : ICommand<CrewMembershipDto>;

// ReSharper disable once UnusedType.Global
public class DeleteCrewMembershipCommandValidator : AbstractValidator<DeleteCrewMembershipCommand>
{
    public DeleteCrewMembershipCommandValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0)
            .WithMessage("CrewMembership ID must be greater than 0");
    }
}

internal sealed class DeleteCrewMembershipCommandHandler : ICommandHandler<DeleteCrewMembershipCommand, CrewMembershipDto>
{
    private readonly ICrewMembershipRepository _crewMembershipRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly CrewMembershipMapper _mapper;
    private readonly TimeProvider _timeProvider;

    public DeleteCrewMembershipCommandHandler(
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

    public async Task<CrewMembershipDto> Handle(DeleteCrewMembershipCommand request, CancellationToken cancellationToken)
    {
        var entity = await _crewMembershipRepository.GetByIdAsync(request.Id, cancellationToken);

        if (entity is null)
            throw new ResourceNotFoundException(CrewMembershipErrors.NotFound);

        var now = _timeProvider.GetLocalDateTimeNowKindUtc();
        entity.ActiveTo = now;
        entity.IsActive = false;
        entity.IsDeleted = true;
        entity.UpdatedAt = now;

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return _mapper.Map(entity);
    }
}