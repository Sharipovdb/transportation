using System.Security.Claims;
using Transportation.Mediator.Helper.Common.Models;
using Transportation.Mediator.Helper.Exceptions;

namespace Transportation.Shared.Middlewares;

public static class ClaimsPrincipalExtensions
{
    public static bool TryGetUserId(this ClaimsPrincipal user, out long userId)
    {
        userId = 0;

        var value = user.FindFirstValue(ClaimTypes.NameIdentifier);

        return !string.IsNullOrWhiteSpace(value)
               && long.TryParse(value, out userId);
    }

    public static long GetUserId(this ClaimsPrincipal user)
    {
        if (!user.TryGetUserId(out var userId))
        {
            throw new UnauthorizedException(
                new Error(
                    "Auth.UserNull",
                    "Не удалось получить идентификатор пользователя."
                )
            );
        }

        return userId;
    }
}