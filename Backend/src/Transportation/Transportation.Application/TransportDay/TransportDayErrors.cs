using Transportation.Mediator.Helper.Common.Models;

namespace Transportation.Application.TransportDay;

public static class TransportDayErrors
{
    public static readonly Error NotFound = new(
        "TransportDay.NotFound",
        "The requested transport day was not found."
    );

    public static readonly Error AlreadyExists = new(
        "TransportDay.AlreadyExists",
        "A transport day already exists for this crew and date."
    );

    public static readonly Error TaxiLegWithoutFare = new(
        "TransportDay.TaxiLegWithoutFare",
        "Every leg travelled by taxi needs its fare, so the ride can be reimbursed.");

    public static readonly Error TaxiFareOnNonTaxiLeg = new(
        "TransportDay.TaxiFareOnNonTaxiLeg",
        "A taxi fare was given for a leg that was not travelled by taxi.");

    public static readonly Error FareAlreadyRuledOn = new(
        "TransportDay.FareAlreadyRuledOn",
        "This day's taxi fare has already been approved or paid — reject it first.");
}
