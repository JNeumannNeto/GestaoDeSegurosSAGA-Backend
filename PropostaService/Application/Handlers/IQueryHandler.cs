using PropostaService.Application.Queries;

namespace PropostaService.Application.Handlers;

public interface IQueryHandler<TQuery, TResponse> where TQuery : IQuery<TResponse>
{
    Task<TResponse> HandleAsync(TQuery query);
}
