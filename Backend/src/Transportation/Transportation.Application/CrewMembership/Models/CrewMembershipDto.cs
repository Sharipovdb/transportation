namespace Transportation.Application.CrewMembership.Models;

public class CrewMembershipDto
{
    public long Id { get; set; }
    public long CrewId { get; set; }
    public long UserId { get; set; }
    public DateTime ActiveFrom { get; set; }
    public DateTime? ActiveTo { get; set; }
}