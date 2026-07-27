using FluentValidation;
using Transportation.Application.Crew.Models;
using Transportation.Application.Crew.Repositories;
using Transportation.Application.Crew.Specification;
using Transportation.Mediator.Helper.Commands;
using Transportation.Mediator.Helper.Exceptions;
using Transportation.Mediator.Helper.Persistence;

namespace Transportation.Application.Crew.Commands;

public record UpdateCrewCommand(
    long Id,
    string? Name,
    long? LeadId,
    long? DriverId,
    long? RouteId,
    int? SeatCapacity
) : ICommand<CrewDto>;

// ReSharper disable once UnusedType.Global
public sealed class UpdateCrewCommandValidator : AbstractValidator<UpdateCrewCommand>
{
    public UpdateCrewCommandValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0)
            .WithMessage("CrewId must be greater than 0");
    }
}

internal sealed class UpdateCrewCommandHandler : ICommandHandler<UpdateCrewCommand, CrewDto>
{
    private readonly ICrewRepository _crewRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly CrewMapper _crewMapper;

    public UpdateCrewCommandHandler(
        ICrewRepository crewRepository,
        IUnitOfWork unitOfWork,
        CrewMapper crewMapper)
    {
        _crewRepository = crewRepository;
        _unitOfWork = unitOfWork;
        _crewMapper = crewMapper;
    }

    public async Task<CrewDto> Handle(UpdateCrewCommand request, CancellationToken cancellationToken)
    {
        var spec = new CrewByIdSpec(request.Id);
        var entity = await _crewRepository.FirstOrDefaultAsync(spec, cancellationToken);

        if (entity is null)
            throw new BusinessLogicException(CrewErrors.NotFound);

        if (request.SeatCapacity is not null && (request.SeatCapacity < 3 || request.SeatCapacity > 7))
            throw new BusinessLogicException(CrewErrors.SeatCapacityOutOfRange);
        
        if (request.LeadId is not null && request.DriverId is not null)
            throw new BusinessLogicException(CrewErrors.LeadIdDriverLeadIdAssigned);

        var crewSpec = new CrewByLeadId(request.LeadId, request.DriverId, excludeCrewId: request.Id);
        var exist = await _crewRepository.AnyAsync(crewSpec, cancellationToken);

        if (exist)
            throw new BusinessLogicException(CrewErrors.LeaderAlreadyAssigned);
        
        if (request.Name is not null)
            entity.Name = request.Name;

        if (request.LeadId.HasValue)
            entity.CrewLeadId = request.LeadId.Value;

        if (request.DriverId.HasValue)
            entity.DriverLeadId = request.DriverId.Value;

        if (request.RouteId.HasValue)
            entity.RouteId = request.RouteId.Value;

        if (request.SeatCapacity is not null)
            entity.SeatCapacity = request.SeatCapacity.Value;

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return _crewMapper.Map(entity);
    }
}