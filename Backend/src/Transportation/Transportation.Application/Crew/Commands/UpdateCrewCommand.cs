using FluentValidation;
using Transportation.Application.Crew.Models;
using Transportation.Application.Crew.Repositories;
using Transportation.Application.Crew.Specification;
using Transportation.Mediator.Helper.Commands;
using Transportation.Mediator.Helper.Common.Extensions;
using Transportation.Mediator.Helper.Exceptions;
using Transportation.Mediator.Helper.Persistence;

namespace Transportation.Application.Crew.Commands;

public record UpdateCrewCommand(
    long Id,
    string? Name,
    long? CrewLeadId,
    long? DriverLeadId,
    long? RouteId,
    int? SeatCapacity
) : ICommand<CrewDto>;

// ReSharper disable once UnusedType.Global
public sealed class UpdateCrewCommandValidator : AbstractValidator<UpdateCrewCommand>
{
    public UpdateCrewCommandValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("CrewId must be greater than 0");

        When(x => !string.IsNullOrWhiteSpace(x.Name), () =>
        {
            RuleFor(x => x.Name!)
                .MaximumLength(20).WithMessage("Maximum length is 20");
        });

        When(x => x.SeatCapacity.HasValue, () =>
        {
            RuleFor(x => x.SeatCapacity!.Value)
                .InclusiveBetween(3, 7)
                .WithMessage(CrewErrors.SeatCapacityOutOfRange.Description);
        });

        RuleFor(x => x)
            .Must(x => !(x.DriverLeadId.HasValue && x.CrewLeadId.HasValue))
            .WithMessage(CrewErrors.BothLeadsAssigned.Description);
    }
}

internal sealed class UpdateCrewCommandHandler : ICommandHandler<UpdateCrewCommand, CrewDto>
{
    private readonly ICrewRepository _crewRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly CrewMapper _crewMapper;
    private readonly TimeProvider _timeProvider;

    public UpdateCrewCommandHandler(
        ICrewRepository crewRepository,
        IUnitOfWork unitOfWork,
        CrewMapper crewMapper,
        TimeProvider timeProvider)
    {
        _crewRepository = crewRepository;
        _unitOfWork = unitOfWork;
        _crewMapper = crewMapper;
        _timeProvider = timeProvider;
    }

    public async Task<CrewDto> Handle(UpdateCrewCommand request, CancellationToken cancellationToken)
    {
        var entity = await _crewRepository.GetByIdAsync(request.Id, cancellationToken);

        if (entity is null)
            throw new ResourceNotFoundException(CrewErrors.NotFound);

        if (!string.IsNullOrWhiteSpace(request.Name))
        {
            var isNameTaken = await _crewRepository.AnyAsync(
                new CrewExistsSpec(request.Name, excludeCrewId: request.Id), cancellationToken);

            if (isNameTaken)
                throw new BusinessLogicException(CrewErrors.AlreadyExists);

            entity.Name = request.Name;
        }

        if (request.CrewLeadId.HasValue || request.DriverLeadId.HasValue)
        {
            var isLeaderAssigned = await _crewRepository.AnyAsync(
                new CrewByLeadsId(request.CrewLeadId, request.DriverLeadId, excludeCrewId: request.Id), cancellationToken);

            if (isLeaderAssigned)
                throw new BusinessLogicException(CrewErrors.LeaderAlreadyAssigned);
        }

        if (request.CrewLeadId.HasValue)
        {
            entity.CrewLeadId = request.CrewLeadId.Value;
            entity.DriverLeadId = null; 
        }
        else if (request.DriverLeadId.HasValue)
        {
            entity.DriverLeadId = request.DriverLeadId.Value;
            entity.CrewLeadId = null; 
        }

        if (request.RouteId.HasValue)
            entity.RouteId = request.RouteId.Value;

        if (request.SeatCapacity.HasValue)
            entity.SeatCapacity = request.SeatCapacity.Value;

        entity.UpdatedAt = _timeProvider.GetLocalDateTimeNowKindUtc();

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return _crewMapper.Map(entity);
    }
}