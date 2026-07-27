namespace Transportation.API.Models;

public record UpdateCrewMembershipRequest(DateTime? ActiveFrom, DateTime? ActiveTo);