namespace Transportation.Domain.Entities;

public class CrewMembership : BaseEntity
{
    public long CrewId { get; set; }
    public Crew Crew { get; set; }
    
    public long UserId { get; set; }
    public User User { get; set; }
    
    public DateTime ActiveFrom { get; set; }
    public DateTime? ActiveTo { get; set; }
    public bool IsActive { get; set; }
}   