using FluentValidation;
using Transportation.Application.Common.Interfaces;
using Transportation.Application.Crew.Models;
using Transportation.Application.Crew.Repositories;
using Transportation.Application.Crew.Specification;
using Transportation.Application.CrewMembership.Repositories;
using Transportation.Mediator.Helper.Commands;
using Transportation.Mediator.Helper.Common.Extensions;
using Transportation.Mediator.Helper.Exceptions;
using Transportation.Mediator.Helper.Persistence;
using Transportation.Shared.Authorization;

namespace Transportation.Application.Crew.Commands;
public record CreateCrewCommand(
    string Name,
    long RouteId,
    long? DriverLeadId,
    long? CrewLeadId,
    int SeatCapacity
) : ICommand<CrewDto>;

// ReSharper disable once UnusedType.Global
public class CreateCrewCommandValidator : AbstractValidator<CreateCrewCommand>
{
    public CreateCrewCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required.")
            .MaximumLength(20).WithMessage("Maximum length is 20");

        RuleFor(c => c.RouteId)
            .GreaterThan(0).WithMessage("RouteId must be greater than 0");
        
        RuleFor(x => x)
            .Must(x => !(x.DriverLeadId is null && x.CrewLeadId is null))
            .WithMessage(CrewErrors.BothLeadsNull.Description)
            
            .Must(x => !(x.DriverLeadId.HasValue && x.CrewLeadId.HasValue))
            .WithMessage(CrewErrors.BothLeadsAssigned.Description);
        
        RuleFor(c => c.SeatCapacity)
            .InclusiveBetween(3, 7).WithMessage(CrewErrors.SeatCapacityOutOfRange.Description);
    }
}

internal sealed class CreateCrewCommandHandler : ICommandHandler<CreateCrewCommand, CrewDto>
{
    private readonly ICrewRepository _crewRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly CrewMapper _crewMapper;
    private readonly TimeProvider _timeProvider;
    private readonly ICrewMembershipRepository _crewMembershipRepository;
    private readonly IRoleService _roleService;

    public CreateCrewCommandHandler(
        ICrewRepository crewRepository,
        IUnitOfWork unitOfWork,
        CrewMapper crewMapper,
        TimeProvider timeProvider,
        ICrewMembershipRepository crewMembershipRepository,
        IRoleService roleService)
    {
        _crewRepository = crewRepository;
        _unitOfWork = unitOfWork;
        _crewMapper = crewMapper;
        _timeProvider = timeProvider;
        _crewMembershipRepository = crewMembershipRepository;
        _roleService = roleService;
    }

    public async Task<CrewDto> Handle(CreateCrewCommand request, CancellationToken cancellationToken)
    {
        var now = _timeProvider.GetLocalDateTimeNowKindUtc();
        
        var spec = new CrewExistsSpec(request.Name);
        var exists = await _crewRepository.AnyAsync(spec, cancellationToken);

        if (exists)
            throw new BusinessLogicException(CrewErrors.AlreadyExists);
        
        var crewSpec = new CrewByLeadsId(request.CrewLeadId, request.DriverLeadId);
        var isLeaderAlreadyAssigned = await _crewRepository.AnyAsync(crewSpec, cancellationToken);
       
        if (isLeaderAlreadyAssigned)
            throw new BusinessLogicException(CrewErrors.LeaderAlreadyAssigned);
        
        var entity = new Domain.Entities.Crew
        {
            Name = request.Name,
            RouteId = request.RouteId,
            CrewLeadId = request.CrewLeadId,
            DriverLeadId = request.DriverLeadId,
            SeatCapacity = request.SeatCapacity,
            CreatedAt = now, 
            UpdatedAt = now
        };
        
        await _crewRepository.AddAsync(entity, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        
        var leaderUserId = request.CrewLeadId ?? request.DriverLeadId;

        if (leaderUserId.HasValue)
        {
            var membership = new Domain.Entities.CrewMembership
            {
                CrewId = entity.Id, 
                UserId = leaderUserId.Value,
                ActiveFrom = now,
                IsActive = true,
                CreatedAt = now,
                UpdatedAt = now
            };
            
            await _crewMembershipRepository.AddAsync(membership, cancellationToken);
        }

        if (request.CrewLeadId.HasValue)
            await _roleService.AssignAsync(RoleNames.CrewLead, request.CrewLeadId.Value);

        else if (request.DriverLeadId.HasValue)
            await _roleService.AssignAsync(RoleNames.DriverLead, request.DriverLeadId.Value);
        
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return _crewMapper.Map(entity);
    }
}