namespace Transportation.API.Models;

public class UpdateCrewRequest
{
    public string? Name { get; set; }
    public long? LeadId { get; set; }
    public long? DriverId { get; set; }
    public long? RouteId { get; set; }
    public int? SeatCapacity { get; set; }
}