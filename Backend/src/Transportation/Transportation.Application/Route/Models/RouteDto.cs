namespace Transportation.Application.Route.Models;

public class RouteDto
{
    public long Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public double DistanceKm { get; set; }
}