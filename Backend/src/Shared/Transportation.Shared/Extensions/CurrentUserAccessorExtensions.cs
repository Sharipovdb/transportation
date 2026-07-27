using System.Security.Claims;
using Transportation.Mediator.Helper.Exceptions;

namespace Transportation.Shared.Extensions;

/// <summary>
/// Методы расширения для <see cref="CurrentUserAccessor"/>.
/// </summary>
public static class CurrentUserAccessorExtensions
{
    /// <param name="accessor">Экземпляр <see cref="ICurrentUserAccessor"/>.</param>
    extension(ICurrentUserAccessor accessor)
    {
        /// <summary>
        /// Возвращает текущего пользователя. Выбрасывает исключение если он не задан.
        /// </summary>
        /// <returns>Текущий пользователь.</returns>
        /// <exception cref="UnauthorizedException"></exception>
        public ClaimsPrincipal GetRequiredUser()
        {
            ArgumentNullException.ThrowIfNull(accessor);

            return accessor.User ?? throw new UnauthorizedException();
        }

        /// <summary>
        /// Устанавливает текущего пользователя.
        /// </summary>
        /// <remarks>
        /// После вызова метода <see cref="IDisposable.Dispose"/> свойство <see cref="ICurrentUserAccessor.User"/>
        /// будет установлено в <see langword="null"/>.
        /// </remarks>
        /// <param name="user">Пользователь, который будет установлен текущим для текущего контекста выполнения.</param>
        /// <returns><see cref="IDisposable"/> представляющий завершение блока установки текущего пользователя.</returns>
        public IDisposable BeginScope(ClaimsPrincipal user)
        {
            ArgumentNullException.ThrowIfNull(accessor);
            ArgumentNullException.ThrowIfNull(user);

            accessor.User = user;
            return new CurrentUserScope(accessor);
        }
    }

    private sealed class CurrentUserScope : IDisposable
    {
        private readonly ICurrentUserAccessor _accessor;

        public CurrentUserScope(ICurrentUserAccessor accessor)
        {
            _accessor = accessor;
        }

        public void Dispose()
        {
            _accessor.User = null;
        }
    }
}
