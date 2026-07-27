using Transportation.Mediator.Helper.Queries;
using Transportation.Application.Crew.Models;
using Transportation.Application.Crew.Repositories;
using Transportation.Application.Crew.Specification;
using Transportation.Mediator.Helper.Exceptions;

namespace Transportation.Application.Crew.Queries;

public sealed record GetCrewByIdQuery(long Id) : IQuery<CrewDto>;

internal sealed class GetCrewByIdQueryHandler : IQueryHandler<GetCrewByIdQuery, CrewDto>
{
    private readonly ICrewRepository _repository;
    private readonly CrewMapper _mapper;

    public GetCrewByIdQueryHandler(ICrewRepository repository, CrewMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<CrewDto> Handle(GetCrewByIdQuery request, CancellationToken cancellationToken)
    {
        var spec = new CrewByIdSpec(request.Id, asNoTracking: true);
        var entity = await _repository.FirstOrDefaultAsync(spec, cancellationToken);

        if (entity is null)
            throw new ResourceNotFoundException(CrewErrors.NotFound);

        return _mapper.Map(entity);
    }
}