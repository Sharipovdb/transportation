namespace Transportation.Domain.Entities;

public class Crew : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public long RouteId { get; set; }
    public Route Route { get; set; } = null!;
    // A crew is led either by a driver-lead, who drives it in their own car, or by a
    // manager-lead, who arranges taxis for it — never both, and the unset one is null.
    public long? CrewLeadId { get; set; }
    public User? CrewLead { get; set; }
    public long? DriverLeadId { get; set; }
    public User? DriverLead { get; set; }
    public int SeatCapacity  { get; set; }
}