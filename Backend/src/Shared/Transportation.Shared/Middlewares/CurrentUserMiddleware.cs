using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Transportation.Shared.Extensions;

namespace Transportation.Shared.Middlewares;

/// <summary>
/// Представляет middleware для установки текущего пользователя в <see cref="ICurrentUserAccessor" />.
/// </summary>
/// <remarks>
/// Текущий пользователь используется из <see cref="HttpContext.User" />.
/// </remarks>
public class CurrentUserMiddleware : IMiddleware
{
    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        var currentUserAccessor = context.RequestServices.GetRequiredService<ICurrentUserAccessor>();
        using var _ = currentUserAccessor.BeginScope(context.User);

        await next(context);
    }
}