namespace Transportation.Application.Crew.Models;

public class CrewDto 
{
    public long Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public long RouteId { get; set; }
    public long? DriverLeadId { get; set; }
    public long? CrewLeadId { get; set; }
    public int SeatCapacity { get; set; }
}   