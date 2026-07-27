using Transportation.Mediator.Helper.Common.Models;

namespace Transportation.Application.Crew;

public static class CrewErrors
{
    public static readonly Error AlreadyExists = new(
        "Crew.AlreadyExists",
        "A crew already exists."
    );

    public static readonly Error NotFound = new(
        "Crew.NotFound",
        "The requested crew was not found."
    );

    public static readonly Error LeadIdDriverLeadIdAssigned = new(
        "LeadId.DriverLeadId_Assigned",
        "LeadId and DriverLeadId cannot both be assigned."
    );

    public static readonly Error LeaderAlreadyAssigned = new(
        "Crew.LeaderAlreadyAssigned",
        "The leader is already assigned to a driver lead."
    );
    
    public static readonly Error LeadIdDriverLeadIdIsNull = new(
        "Crew.LeadIdDriverLeadId_Null",
        "LeadId and DriverLeadId cannot both be null."
    );

    public static readonly Error SeatCapacityOutOfRange = new(
        "Crew.SeatCapacityOutOfRange",
        "Seat capacity should be between 3 and 7."
    );
}