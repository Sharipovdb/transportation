using Transportation.Application.Vehicle.Models;
using Transportation.Application.Vehicle.Repositories;
using Transportation.Application.Vehicle.Specifications;
using Transportation.Mediator.Helper.Exceptions;
using Transportation.Mediator.Helper.Queries;

namespace Transportation.Application.Vehicle.Queries;

public sealed record GetVehicleByDriverId(long DriverId) : IQuery<VehicleDto>;
    
internal sealed class GetVehicleByDriverIdHandler : IQueryHandler<GetVehicleByDriverId, VehicleDto>
{
    private readonly IVehicleRepository _vehicleRepository;
    private readonly VehicleMapper _mapper;

    public GetVehicleByDriverIdHandler(IVehicleRepository vehicleRepository, VehicleMapper maper)
    {
        _vehicleRepository = vehicleRepository;
        _mapper = maper;
    }

    public async Task<VehicleDto> Handle(GetVehicleByDriverId request, CancellationToken cancellationToken)
    {
        var spec = new VehicleByDriverIdSpec(request.DriverId);
        var entity = await _vehicleRepository.FirstOrDefaultAsync(spec, cancellationToken);

        if (entity is null)
            throw new ResourceNotFoundException(VehicleErrors.NotFound);

        return _mapper.Map(entity);
    }
}