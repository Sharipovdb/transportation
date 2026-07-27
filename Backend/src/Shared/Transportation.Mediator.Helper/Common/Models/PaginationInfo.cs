namespace Transportation.Mediator.Helper.Common.Models;

/// <summary>
/// Содержит параметры пагинации для постраничного получения данных.
/// </summary>
/// <remarks>
/// Используется при запросе списков, чтобы определить номер страницы и количество элементов на странице.
/// </remarks>
/// <param name="Index">Индекс страницы</param>
/// <param name="Size">Количество элементов, которые должны быть возвращены на одной странице.</param>
public sealed record PaginationInfo(int Index, int Size);