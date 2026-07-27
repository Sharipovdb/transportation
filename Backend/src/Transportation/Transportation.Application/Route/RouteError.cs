using Transportation.Mediator.Helper.Common.Models;

namespace Transportation.Application.Route;

public static class RouteError
{
    public static readonly Error NotFound = new(
        "Rout.NotFound",
        "The specific rout does not exist."
        );

    public static readonly Error AlreadyExist = new(
        "Rout.AlreadyExist",
        "The specific rout already exists."
    );
}