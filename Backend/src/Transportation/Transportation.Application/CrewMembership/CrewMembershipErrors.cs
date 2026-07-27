using Transportation.Mediator.Helper.Common.Models;

namespace Transportation.Application.CrewMembership;

public static class CrewMembershipErrors
{
    public static readonly Error AlreadyExists = new(
        "CrewMembership.AlreadyExists",
        "A CrewMembership alignment already exists for this employee and crew."
    );

    public static readonly Error NotFound = new(
        "CrewMembership.NotFound",
        "The requested CrewMembership was not found."
    );

    public static readonly Error FullSeatCapacity = new(
        "CrewMembership.FullSeatCapacity",
        "CrewMembership SeatCapacity is full."
    );

    public static readonly Error EmployeerIsAsigned = new(
        "CrewMembership.EmployeerIsAsigned",
        "CrewMembership Employee is assigned to a crew."
        );

    public static readonly Error ActiveToUnderLine = new(
        "CrewMembership.ActiveToUnderLine",
        "CrewMembership ActiveToUnderLine");

    public static readonly Error ActiveFromUnderLine = new(
        "CrewMembership.ActiveFromUnderLine",
        "CrewMembership ActiveFromUnderLine");
}