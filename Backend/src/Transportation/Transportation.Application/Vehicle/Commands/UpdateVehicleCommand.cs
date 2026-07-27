using Transportation.Application.Vehicle.Models;
using Transportation.Application.Vehicle.Repositories;
using Transportation.Application.Vehicle.Specifications;
using Transportation.Mediator.Helper.Commands;
using Transportation.Mediator.Helper.Exceptions;
using Transportation.Mediator.Helper.Persistence;

namespace Transportation.Application.Vehicle.Commands;

public sealed record UpdateVehicleCommand(
    long Id,
    long DriverId,
    string Plate,
    int SeatCount,
    decimal AmortizationBasis
) : ICommand<VehicleDto>;

internal sealed class UpdateVehicleCommandHandler : ICommandHandler<UpdateVehicleCommand, VehicleDto>
{
    private readonly IVehicleRepository _vehicleRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly VehicleMapper _mapper;

    public UpdateVehicleCommandHandler(
        IVehicleRepository vehicleRepository,
        IUnitOfWork unitOfWork,
        VehicleMapper mapper)
    {
        _vehicleRepository = vehicleRepository;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<VehicleDto> Handle(UpdateVehicleCommand request, CancellationToken cancellationToken)
    {
        var spec = new VehicleByIdSpec(request.Id);
        var entity = await _vehicleRepository.FirstOrDefaultAsync(spec, cancellationToken);

        if (entity is null)
            throw new ResourceNotFoundException(VehicleErrors.NotFound);

        entity.DriverId = request.DriverId;
        entity.Plate = request.Plate;
        entity.SeatCount = request.SeatCount;
        entity.AmortizationBasis = request.AmortizationBasis;


        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return _mapper.Map(entity);
    }
}