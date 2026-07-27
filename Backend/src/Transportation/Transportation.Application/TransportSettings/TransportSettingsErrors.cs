using Transportation.Mediator.Helper.Common.Models;

namespace Transportation.Application.TransportSettings;

public static class TransportSettingsErrors
{
    public static readonly Error NotFound = new(
        "TransportSettings.NotFound",
        "Transport settings not found."
    );
}