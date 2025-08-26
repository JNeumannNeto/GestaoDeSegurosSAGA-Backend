using PropostaService.Application.DTOs;

namespace PropostaService.Application.Queries;

public record ListarPropostasQuery() : IQuery<IEnumerable<PropostaResponse>>;
