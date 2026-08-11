namespace Transportation.API.Models;

public sealed class UpdatePayoutLineRequest
{
    public double DriverKm { get; set; }
    public double ExtraBusinessKm { get; set; }
    public decimal TaxiCompensation { get; set; }
}
