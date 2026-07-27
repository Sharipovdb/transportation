public sealed class UpdateVehicleRequest
{
    public long driver_id{get; set;}
    public string plate {get; set;}=string.Empty;
    public int seat_count {get; set;}
    public decimal amortization_basis {get; set;}
}