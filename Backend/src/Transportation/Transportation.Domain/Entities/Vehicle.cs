namespace Transportation.Domain.Entities
{
    public class Vehicle : BaseEntity
    {
        public long DriverId { get; set; }
        public User Driver { get; set; } = null!;

        public string Plate { get; set; } = string.Empty;
        public int SeatCount { get; set; }
        public decimal AmortizationBasis { get; set; }
    }
}