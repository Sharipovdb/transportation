using Transportation.Application.CrewMembership.Repositories;
using Transportation.Mediator.Helper.Exceptions;
using Transportation.Mediator.Helper.Persistence;
using Transportation.Application.CrewMembership.Models;
using Transportation.Application.CrewMembership.Specification;
using Transportation.Mediator.Helper.Commands;
using Transportation.Mediator.Helper.Common.Extensions;

namespace Transportation.Application.CrewMembership.Commands;

public sealed record DeleteCrewMembership(long Id) : ICommand<CrewMembershipDto>;

internal sealed class DeleteCrewMembershipHandler : ICommandHandler<DeleteCrewMembership, CrewMembershipDto>
{
    private readonly ICrewMembershipRepository _crewMembershipRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly CrewMembershipMapper _mapper;
    private readonly TimeProvider _timeProvider;

    public DeleteCrewMembershipHandler(
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

    public async Task<CrewMembershipDto> Handle(DeleteCrewMembership request, CancellationToken cancellationToken)
    {
        var spec = new CrewMembershipByIdSpec(request.Id);
        var entity = await _crewMembershipRepository.FirstOrDefaultAsync(spec, cancellationToken);

        if (entity is null)
            throw new ResourceNotFoundException(CrewMembershipErrors.NotFound);

        entity.ActiveTo = _timeProvider.GetLocalDateTimeNowKindUtc();
        entity.IsActive = false;
        entity.IsDeleted = true;

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return _mapper.Map(entity);
    }
}