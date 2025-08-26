using PropostaService.Application.Commands;

namespace PropostaService.Application.Handlers;

public interface ICommandHandler<TCommand, TResponse> where TCommand : ICommand<TResponse>
{
    Task<TResponse> HandleAsync(TCommand command);
}
