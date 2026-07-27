namespace Transportation.Application.Vehicle.Models;

    public class VehicleDto
    {
        public long Id {get; set;}
        public long DriverId { get; set; }
        public string Plate { get; set; }
        public int SeatCount { get; set; }
        public decimal AmortizationBasis { get; set; }
    }