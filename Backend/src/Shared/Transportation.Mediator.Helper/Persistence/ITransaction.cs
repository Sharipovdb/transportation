namespace Transportation.Mediator.Helper.Persistence;

/// <summary>
/// Представляет абстракцию над транзакцией базы данных.
/// </summary>
public interface ITransaction : IDisposable, IAsyncDisposable
{
    /// <summary>
    /// Завершает транзакцию с успехом.
    /// </summary>
    /// <param name="cancellationToken"></param>
    Task CommitAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Откатывает изменения транзакции.
    /// </summary>
    /// <param name="cancellationToken"></param>
    Task RollbackAsync(CancellationToken cancellationToken = default);
}