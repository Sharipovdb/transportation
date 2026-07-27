namespace Transportation.API.Models;

public sealed class UpdateTransportDayRequest
{
    public Domain.Entities.TransportMode? MorningMode { get; set; }
    public Domain.Entities.TransportMode? AfternoonMode { get; set; }
    public long? DriverId { get; set; }
    public double? CommuteKm { get; set; }
    public double? ExtraBusinessKm { get; set; }
    public string? Notes { get; set; }
}