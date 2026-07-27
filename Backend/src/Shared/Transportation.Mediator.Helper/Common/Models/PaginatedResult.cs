namespace Transportation.Mediator.Helper.Common.Models;

/// <summary>
/// Модель представляющая результат постраничной загрузки.
/// </summary>
/// <typeparam name="T">Тип загруженное сущности.</typeparam>
public sealed record PaginatedResult<T>(List<T> Items, long TotalCount);