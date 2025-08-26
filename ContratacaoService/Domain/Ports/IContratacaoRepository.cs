using ContratacaoService.Domain.Common;
using ContratacaoService.Domain.Entities;

namespace ContratacaoService.Domain.Ports;

public interface IContratacaoRepository
{
    Task<Contratacao?> GetByIdAsync(Guid id);
    Task<PagedResult<Contratacao>> GetAllAsync(PagedRequest request);
    Task<Contratacao?> GetByPropostaIdAsync(Guid propostaId);
    Task AddAsync(Contratacao contratacao);
}
