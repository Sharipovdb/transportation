using Transportation.Mediator.Helper.Common.Models;

namespace Transportation.Application.MonthlyTransportSheet;

public static class MonthlyTransportSheetErrors
{
    public static readonly Error AlreadyExists = new(
        "MonthlyTransportSheet.AlreadyExists",
        "A sheet already exists for this crew and month."
    );

    public static readonly Error NotFound = new(
        "MonthlyTransportSheet.NotFound",
        "Monthly transport sheet not found."
    );

    public static readonly Error AlreadyConfirmed = new(
        "MonthlyTransportSheet.AlreadyConfirmed",
        "Monthly transport sheet already confirmed."
    );
}