namespace Transportation.API.Models;

public class UpdateTransportAttendanceRequest
{
    public long? TransportDayId { get; set; }
    public long? UserId { get; set; }
    public bool? IsPresent { get; set; }
}