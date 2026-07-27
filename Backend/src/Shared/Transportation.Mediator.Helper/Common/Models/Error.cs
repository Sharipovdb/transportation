namespace Transportation.Mediator.Helper.Common.Models;

/// <summary>
/// Модель представляющая информацию об ошибке.
/// </summary>
/// <param name="Code">Код ошибки.</param>
/// <param name="Description">Текстовое описание ошибки.</param>
public sealed record Error(string Code, string Description);