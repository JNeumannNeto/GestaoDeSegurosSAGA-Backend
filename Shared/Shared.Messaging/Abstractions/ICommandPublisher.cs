using Shared.Messaging.Commands;

namespace Shared.Messaging.Abstractions;

public interface ICommandPublisher
{
    Task PublishAsync<T>(T command) where T : ICommand;
}
