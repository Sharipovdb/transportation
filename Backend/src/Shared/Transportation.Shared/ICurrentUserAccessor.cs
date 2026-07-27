using System.Security.Claims;

namespace Transportation.Shared;

/// <summary>
/// Предоставляет доступ к текущему пользователю, если он задан.
/// </summary>
public interface ICurrentUserAccessor
{
    /// <summary>
    /// Возвращает или устанавливает текущий <see cref="ClaimsPrincipal" />,
    /// представляющий текущего пользователя.
    /// Возвращает <see langword="null" /> если текущий пользователь не задан.
    /// </summary>
    ClaimsPrincipal? User { get; set; }
}

/// <summary>
///     Представляет реализацию <see cref="ICurrentUserAccessor" /> на основании текущего контекста выполнения.
/// </summary>
/// <remarks>
///     Основан на реализации IHttpContextAccessor от Microsoft.
/// </remarks>
public sealed class CurrentUserAccessor : ICurrentUserAccessor
{
    private static readonly AsyncLocal<CurrentUserHolder> _current = new();

    /// <inheritdoc />
    public ClaimsPrincipal? User
    {
        get => _current.Value?.User;
        set
        {
            var holder = _current.Value;
            // Clear current User trapped in the AsyncLocals, as it's done.

            holder?.User = null;

            if (value is not null)
            {
                // Use an object indirection to hold the User in the AsyncLocal,
                // so it can be cleared in all ExecutionContexts when it's cleared.
                _current.Value = new CurrentUserHolder
                {
                    User = value,
                };
            }
        }
    }

    private sealed class CurrentUserHolder
    {
        public ClaimsPrincipal? User;
    }
}