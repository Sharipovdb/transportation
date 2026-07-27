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
}