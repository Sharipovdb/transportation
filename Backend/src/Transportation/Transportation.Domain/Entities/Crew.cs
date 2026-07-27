namespace Transportation.Domain.Entities;

public class Crew : BaseEntity
{
    public string Name { get; set; }
    public long RouteId { get; set; }
    public Route Route { get; set; } 
    public long? CrewLeadId { get; set; }
    public User CrewLead { get; set; }
    public long? DriverLeadId { get; set; }
    public User DriverLead { get; set; } 
    public int SeatCapacity  { get; set; }
}