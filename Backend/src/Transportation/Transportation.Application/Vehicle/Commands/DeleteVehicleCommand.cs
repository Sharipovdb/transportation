using Transportation.Application.Vehicle.Repositories;
using Transportation.Application.Vehicle.Specifications;
using Transportation.Mediator.Helper.Commands;
using Transportation.Mediator.Helper.Exceptions;
using Transportation.Mediator.Helper.Persistence;

namespace Transportation.Application.Vehicle.Commands;

public sealed record DeleteVehicleCommand(long Id) : ICommand<bool>;

internal sealed class DeleteVehicleCommandHandler : ICommandHandler<DeleteVehicleCommand, bool>
{
    private readonly IVehicleRepository _vehicleRepository;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteVehicleCommandHandler(
        IVehicleRepository vehicleRepository,
        IUnitOfWork unitOfWork)
    {
        _vehicleRepository = vehicleRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<bool> Handle(DeleteVehicleCommand request, CancellationToken cancellationToken)
    {
        var spec = new VehicleByIdSpec(request.Id);
        var entity = await _vehicleRepository.FirstOrDefaultAsync(spec, cancellationToken);

        if (entity is null )
            throw new ResourceNotFoundException(VehicleErrors.NotFound);

        entity.IsDeleted = true;
        
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return true;
    }
}