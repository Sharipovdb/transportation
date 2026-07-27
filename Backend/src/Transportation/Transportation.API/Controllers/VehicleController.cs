using MediatR;
using Microsoft.AspNetCore.Mvc;
using Transportation.Application.Vehicle.Commands;
using Transportation.Application.Vehicle.Models;
using Transportation.Application.Vehicle.Queries;
using Transportation.Mediator.Helper.Common.Models;

namespace Transportation.API.Controllers;

public class VehicleController : BaseController
{
    public VehicleController(IMediator mediator) : base(mediator)
    {
    }

    [HttpGet]
    public async Task<PaginatedResult<VehicleDto>> GetAll(
        [FromQuery] GetAllVehicles query,
        CancellationToken cancellationToken = default)
    {
        return await _mediator.Send(query, cancellationToken);
    }

    [HttpGet("{id:long}")]
    public async Task<VehicleDto> GetById(
        long id,
        CancellationToken cancellationToken = default)
    {
        return await _mediator.Send(new GetVehicleById(id), cancellationToken);
    }

    [HttpPost]
    public async Task<VehicleDto> Create(
        [FromBody] CreateVehicleCommand command,
        CancellationToken cancellationToken = default)
    {
        return await _mediator.Send(command, cancellationToken);
    }

    [HttpPut("{id:long}")]
    public async Task<VehicleDto> Update(
        long id,
        [FromBody] UpdateVehicleRequest request,
        CancellationToken cancellationToken = default)
    {
        var command = new UpdateVehicleCommand(
            id,
            request.driver_id,
            request.plate,
            request.seat_count,
            request.amortization_basis
        );

        return await _mediator.Send(command, cancellationToken);
    }
    
    [HttpGet("{driverId:long}")]
    public async Task<VehicleDto> GetByDriverId(
        long driverId,
        CancellationToken cancellationToken = default)
    {
        return await _mediator.Send(new GetVehicleByDriverId(driverId), cancellationToken);
    }
     
    [HttpDelete("{id:long}")]
    public async Task<bool> Delete(
        long id,
        CancellationToken cancellationToken = default)
    {
        return await _mediator.Send(new DeleteVehicleCommand(id), cancellationToken);
    }
}