using Shared.Messaging.Events;

namespace Shared.Messaging.Abstractions;

public interface IEventPublisher
{
    Task PublishAsync<T>(T @event) where T : IEvent;
}
