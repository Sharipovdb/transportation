using FluentValidation;
using Transportation.Application.Vehicle.Models;
using Transportation.Application.Vehicle.Repositories;
using Transportation.Application.Vehicle.Specifications;
using Transportation.Mediator.Helper.Commands;
using Transportation.Mediator.Helper.Common.Extensions;
using Transportation.Mediator.Helper.Exceptions;
using Transportation.Mediator.Helper.Persistence;

namespace Transportation.Application.Vehicle.Commands;

public sealed record CreateVehicleCommand(
    long DriverId,
    string Plate,
    int SeatCount,
    decimal AmortizationBasis
) : ICommand<VehicleDto>;

// ReSharper disable once UnusedType.Global
public sealed class CreateVehicleCommandValidator : AbstractValidator<CreateVehicleCommand>
{
    public CreateVehicleCommandValidator()
    {
        RuleFor(x => x.DriverId)
            .GreaterThan(0);

        RuleFor(x => x.Plate)
            .NotEmpty()
            .MaximumLength(20); 

        RuleFor(x => x.SeatCount)
            .GreaterThan(0);

        RuleFor(x => x.AmortizationBasis)
            .GreaterThanOrEqualTo(0);
    }
}

internal sealed class CreateVehicleCommandHandler : ICommandHandler<CreateVehicleCommand, VehicleDto>
{
    private readonly IVehicleRepository _vehicleRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly VehicleMapper _mapper;
    private readonly TimeProvider _timeProvider;

    public CreateVehicleCommandHandler(
        IVehicleRepository vehicleRepository,
        IUnitOfWork unitOfWork,
        VehicleMapper mapper, TimeProvider timeProvider)
    {
        _vehicleRepository = vehicleRepository;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _timeProvider=timeProvider;
    }

    public async Task<VehicleDto> Handle(CreateVehicleCommand request, CancellationToken cancellationToken)
    {
        var spec = new VehicleByPlateSpec(request.Plate);
        var exists = await _vehicleRepository.AnyAsync(spec, cancellationToken);

        if (exists)
            throw new BusinessLogicException(VehicleErrors.AlreadyExists);

        var entity = new Domain.Entities.Vehicle
        {
            DriverId = request.DriverId,
            Plate = request.Plate,
            SeatCount = request.SeatCount,
            AmortizationBasis = request.AmortizationBasis,
            CreatedAt = _timeProvider.GetLocalDateTimeNowKindUtc(),
        };

        await _vehicleRepository.AddAsync(entity, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return _mapper.Map(entity);
    }
}