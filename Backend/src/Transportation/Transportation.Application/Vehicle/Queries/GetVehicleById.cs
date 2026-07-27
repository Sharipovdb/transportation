using Transportation.Application.Vehicle.Models;
using Transportation.Application.Vehicle.Repositories;
using Transportation.Application.Vehicle.Specifications;
using Transportation.Mediator.Helper.Exceptions;
using Transportation.Mediator.Helper.Queries;

namespace Transportation.Application.Vehicle.Queries;

public sealed record GetVehicleById(long Id)
    : IQuery<VehicleDto>;
    
internal sealed class GetVehicleByIdHandler : IQueryHandler<GetVehicleById, VehicleDto>
{
    private readonly IVehicleRepository _vehicleRepository;
    private readonly VehicleMapper _mapper;

    public GetVehicleByIdHandler(
        IVehicleRepository vehicleRepository,
        VehicleMapper mapper)
    {
        _vehicleRepository = vehicleRepository;
        _mapper = mapper;
    }

    public async Task<VehicleDto> Handle(
        GetVehicleById request,
        CancellationToken cancellationToken)
    {
        var spec = new VehicleByIdSpec(request.Id);
        var entity = await _vehicleRepository.FirstOrDefaultAsync(spec, cancellationToken);

        if (entity is null )
            throw new ResourceNotFoundException(VehicleErrors.NotFound);

        return _mapper.Map(entity);
    }
}