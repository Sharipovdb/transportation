using Transportation.Domain.Entities;

namespace Transportation.API.Models;

public class UpdateTaxiExpenseRequest
{
    public long TransportDayId { get; set; }

    public long PaidById { get; set; }

    public Leg Leg { get; set; }

    public decimal Amount { get; set; }

    public TaxiExpenseStatus TaxiExpenseStatus { get; set; }
}
