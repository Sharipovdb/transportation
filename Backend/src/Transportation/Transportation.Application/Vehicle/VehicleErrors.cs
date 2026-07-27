using Transportation.Mediator.Helper.Common.Models;

namespace Transportation.Application.Vehicle;

public static class VehicleErrors
{
    public static readonly Error AlreadyExists = new(
        "Vehicle.AlreadyExists",
        "A vehicle with this license plate number is already registered!."
    );

    public static readonly Error NotFound = new(
        "Vehicle.NotFound",
        "Vehicle not found."
    );
}