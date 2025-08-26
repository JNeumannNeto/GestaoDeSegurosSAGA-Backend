using System.Collections.Concurrent;
using PropostaService.Domain.Entities;
using PropostaService.Domain.Ports;

namespace PropostaService.Infrastructure.Repositories;

public class PropostaRepository : IPropostaRepository
{
    private readonly ConcurrentDictionary<Guid, Proposta> _propostas = new();

    public Task<Proposta?> GetByIdAsync(Guid id)
    {
        _propostas.TryGetValue(id, out var proposta);
        return Task.FromResult(proposta);
    }

    public Task<IEnumerable<Proposta>> GetAllAsync()
    {
        return Task.FromResult(_propostas.Values.AsEnumerable());
    }

    public Task AddAsync(Proposta proposta)
    {
        if (proposta == null)
            throw new ArgumentNullException(nameof(proposta));

        if (!_propostas.TryAdd(proposta.Id, proposta))
            throw new InvalidOperationException($"Proposta com ID {proposta.Id} já existe");

        return Task.CompletedTask;
    }

    public Task UpdateAsync(Proposta proposta)
    {
        if (proposta == null)
            throw new ArgumentNullException(nameof(proposta));

        _propostas.AddOrUpdate(proposta.Id, proposta, (key, oldValue) => proposta);
        return Task.CompletedTask;
    }
}
