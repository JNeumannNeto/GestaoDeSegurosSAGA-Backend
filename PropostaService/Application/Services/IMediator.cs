using PropostaService.Application.Commands;
using PropostaService.Application.Queries;

namespace PropostaService.Application.Services;

public interface IMediator
{
    Task<TResponse> SendAsync<TResponse>(ICommand<TResponse> command);
    Task<TResponse> SendAsync<TResponse>(IQuery<TResponse> query);
}
