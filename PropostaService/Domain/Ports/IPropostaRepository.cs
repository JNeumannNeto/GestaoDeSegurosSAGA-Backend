using PropostaService.Domain.Entities;

namespace PropostaService.Domain.Ports;

public interface IPropostaRepository
{
    Task<Proposta?> GetByIdAsync(Guid id);
    Task<IEnumerable<Proposta>> GetAllAsync();
    Task AddAsync(Proposta proposta);
    Task UpdateAsync(Proposta proposta);
}
