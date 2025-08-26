using PropostaService.Application.DTOs;

namespace PropostaService.Application.Ports;

public interface IPropostaAppService
{
    Task<PropostaResponse> CriarPropostaAsync(CriarPropostaRequest request);
    Task<PropostaResponse?> ObterPropostaPorIdAsync(Guid id);
    Task<IEnumerable<PropostaResponse>> ListarPropostasAsync();
    Task<PropostaResponse?> AlterarStatusPropostaAsync(Guid id, AlterarStatusRequest request);
}
