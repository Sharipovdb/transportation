namespace Transportation.Application.TransportDay;
using Transportation.Mediator.Helper.Common.Models;
public static class TransportDayErrors
{
    public static readonly Error NotFound = new(
        "TransportDay.NotFound",
        "No confirmed transport days found."
    );
 
    public static readonly Error AlreadyExists = new(
        "TransportDay.AlreadyExists",
        "A transport day already exists for this crew and date."
    );
    public static readonly Error AlreadyConfirmed = new(
        "TransportDay.AlreadyConfirmed",
        "Transport day already confirmed");

    public static readonly Error AlreadyUnConfirmed = new(
        "TransportDay.AlreadyUnConfirmed",
        "Transport day already unconfirmed");

    public static readonly Error TaxiLegWithoutFare = new(
        "TransportDay.TaxiLegWithoutFare",
        "Every leg travelled by taxi needs its fare, so the ride can be reimbursed.");

    public static readonly Error TaxiFareOnNonTaxiLeg = new(
        "TransportDay.TaxiFareOnNonTaxiLeg",
        "A taxi fare was given for a leg that was not travelled by taxi.");

    public static readonly Error ConfirmedDayIsReadOnly = new(
        "TransportDay.ConfirmedDayIsReadOnly",
        "A confirmed transport day cannot be edited — unconfirm it first.");
}