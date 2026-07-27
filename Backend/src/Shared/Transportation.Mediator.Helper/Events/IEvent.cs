using MediatR;

namespace Transportation.Mediator.Helper.Events;

/// <summary>
/// Интерфейс-маркер, представляющий событие в системе.
/// </summary>
public interface IEvent : INotification;

/// <summary>
/// Представляет обработчик события <typeparamref name="TEvent"/>.
/// </summary>
/// <typeparam name="TEvent">Тип события.</typeparam>
public interface IEventHandler<in TEvent> : INotificationHandler<TEvent> where TEvent : IEvent;