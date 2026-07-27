using Ardalis.Specification;
using Transportation.Application.Vehicle.Models;
using Transportation.Application.Vehicle.Repositories;
using Transportation.Mediator.Helper.Common.Extensions;
using Transportation.Mediator.Helper.Common.Models;
using Transportation.Mediator.Helper.Persistence;
using Transportation.Mediator.Helper.Queries;

namespace Transportation.Application.Vehicle.Queries;

public sealed record GetAllVehicles(
    long? DriverId,
    string? Plate,
    int PageIndex,
    int PageSize
) : IQuery<PaginatedResult<VehicleDto>>;

internal sealed class GetAllVehiclesHandler : IQueryHandler<GetAllVehicles, PaginatedResult<VehicleDto>>
{
    private readonly IVehicleRepository _repository;
    private readonly VehicleMapper _mapper;

    public GetAllVehiclesHandler(
        IVehicleRepository repository,
        VehicleMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<PaginatedResult<VehicleDto>> Handle(
        GetAllVehicles request,
        CancellationToken cancellationToken)
    {
        var spec = new ReadOnlySpecification<Domain.Entities.Vehicle>();
        spec.Query.Where(x =>! x.IsDeleted);

        if (request.DriverId.HasValue)
            spec.Query.Where(x => x.DriverId == request.DriverId);

        if (!string.IsNullOrWhiteSpace(request.Plate))
            spec.Query.Where(x => x.Plate.Contains(request.Plate));

        spec.Query.OrderByDescending(x => x.Id);

        spec.Query.WithPagination(new PaginationInfo(request.PageIndex, request.PageSize));

        var items = await _repository.ListAsync(spec, cancellationToken);
        var totalCount = await _repository.CountAsync(spec, cancellationToken);

        var mapped = _mapper.Map(items);

        return new PaginatedResult<VehicleDto>(mapped, totalCount);
    }
}