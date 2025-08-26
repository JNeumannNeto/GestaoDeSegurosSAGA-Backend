using ContratacaoService.Domain.Models;

namespace ContratacaoService.Domain.Ports;

public interface IPropostaServiceClient
{
    Task<PropostaDto?> ObterPropostaPorIdAsync(Guid propostaId);
}
