using PropostaService.Application.DTOs;

namespace PropostaService.Application.Queries;

public record ObterPropostaPorIdQuery(Guid Id) : IQuery<PropostaResponse?>;

public interface IQuery<TResponse>
{
}
