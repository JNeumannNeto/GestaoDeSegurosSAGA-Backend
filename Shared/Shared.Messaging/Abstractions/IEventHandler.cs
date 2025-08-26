using Shared.Messaging.Events;

namespace Shared.Messaging.Abstractions;

public interface IEventHandler<in T> where T : IEvent
{
    Task HandleAsync(T @event);
}
