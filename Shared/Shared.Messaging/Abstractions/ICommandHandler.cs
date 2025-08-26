using Shared.Messaging.Commands;

namespace Shared.Messaging.Abstractions;

public interface ICommandHandler<in T> where T : ICommand
{
    Task HandleAsync(T command);
}
