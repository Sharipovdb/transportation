using Transportation.Mediator.Helper.Common.Models;

namespace Transportation.Application.CrewMembership;

public static class CrewMembershipErrors
{
    public static readonly Error NotFound = new(
        "CrewMembership.NotFound",
        "The requested crew membership was not found."
    );

    public static readonly Error FullSeatCapacity = new(
        "CrewMembership.FullSeatCapacity",
        "The crew has reached its maximum seat capacity."
    );

    public static readonly Error EmployeeIsAssigned = new(
        "CrewMembership.EmployeeIsAssigned",
        "The employee is already assigned to a crew."
    );

    public static readonly Error ActiveToInvalid = new(
        "CrewMembership.ActiveToInvalid",
        "The ActiveTo date is invalid."
    );

    public static readonly Error ActiveFromInvalid = new(
        "CrewMembership.ActiveFromInvalid",
        "The ActiveFrom date is invalid."
    );
}