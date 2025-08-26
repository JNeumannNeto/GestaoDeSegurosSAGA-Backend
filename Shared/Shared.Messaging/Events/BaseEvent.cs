namespace Shared.Messaging.Events;

public abstract class BaseEvent : IEvent
{
    public Guid Id { get; private set; }
    public DateTime OccurredAt { get; private set; }
    public abstract string EventType { get; }

    protected BaseEvent()
    {
        Id = Guid.NewGuid();
        OccurredAt = DateTime.UtcNow;
    }
}
