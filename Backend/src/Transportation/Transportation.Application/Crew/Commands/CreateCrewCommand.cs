using FluentValidation;
using Transportation.Application.Crew.Models;
using Transportation.Application.Crew.Repositories;
using Transportation.Application.Crew.Specification;
using Transportation.Mediator.Helper.Commands;
using Transportation.Mediator.Helper.Common.Extensions;
using Transportation.Mediator.Helper.Exceptions;
using Transportation.Mediator.Helper.Persistence;

namespace Transportation.Application.Crew.Commands;

public record CreateCrewCommand(
    string Name,
    long RouteId,
    long? DriverLeadId,
    long? LeadId,
    int SeatCapacity
) : ICommand<CrewDto>;

// ReSharper disable once UnusedType.Global
public class CreateCrewCommandValidator : AbstractValidator<CreateCrewCommand>
{
    public CreateCrewCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Name is required.")
            .MaximumLength(20)
            .WithMessage("Maximum length is 20");

        RuleFor(c => c.RouteId)
            .GreaterThan(0)
            .WithMessage("RouteId must be greater than 0");
        

        RuleFor(c => c.SeatCapacity)
            .InclusiveBetween(3, 7)
            .WithMessage("Seat capacity must be between 3 and 7");
    }
}

internal sealed class CreateCrewCommandHandler : ICommandHandler<CreateCrewCommand, CrewDto>
{
    private readonly ICrewRepository _crewRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly CrewMapper _crewMapper;
    private readonly TimeProvider _timeProvider;

    public CreateCrewCommandHandler(
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

    public async Task<CrewDto> Handle(CreateCrewCommand request, CancellationToken cancellationToken)
    {
        var spec = new CrewExistsSpec(request.Name, request.LeadId, request.DriverLeadId, request.RouteId);
        var exists = await _crewRepository.AnyAsync(spec, cancellationToken);

        if (exists)
            throw new BusinessLogicException(CrewErrors.AlreadyExists);

        if (request.DriverLeadId is null && request.LeadId is null)
            throw new BusinessLogicException(CrewErrors.LeadIdDriverLeadIdIsNull);

        if (request.DriverLeadId is not null && request.LeadId is not null)
            throw new BusinessLogicException(CrewErrors.LeadIdDriverLeadIdAssigned);
        
        var crewSpec = new CrewByLeadId(request.LeadId, request.DriverLeadId);
        var exist = await _crewRepository.AnyAsync(crewSpec, cancellationToken);
       
        if (exist)
            throw new BusinessLogicException(CrewErrors.LeaderAlreadyAssigned);
        
        var entity = new Domain.Entities.Crew
        {
            Name = request.Name,
            RouteId = request.RouteId,
            CrewLeadId = request.LeadId,
            DriverLeadId = request.DriverLeadId,
            SeatCapacity = request.SeatCapacity,
            CreatedAt = _timeProvider.GetLocalDateTimeNowKindUtc(), 
            UpdatedAt = _timeProvider.GetLocalDateTimeNowKindUtc()
        };

        await _crewRepository.AddAsync(entity, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return _crewMapper.Map(entity);
    }
}