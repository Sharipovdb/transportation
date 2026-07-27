namespace Transportation.Domain.Entities;

public class Crew : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public long RouteId { get; set; }
    public Route Route { get; set; } = null!;
    public long? CrewLeadId { get; set; }
    public User CrewLead { get; set; } = null!;
    public long? DriverLeadId { get; set; }
    public User DriverLead { get; set; } = null!;
    public int SeatCapacity  { get; set; }
}