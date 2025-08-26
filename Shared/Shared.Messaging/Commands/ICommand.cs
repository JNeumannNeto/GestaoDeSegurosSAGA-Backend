namespace Shared.Messaging.Commands;

public interface ICommand
{
    Guid Id { get; }
    DateTime CreatedAt { get; }
    string CommandType { get; }
}
