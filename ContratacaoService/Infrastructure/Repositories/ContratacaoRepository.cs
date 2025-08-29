using System.Collections.Concurrent;
using ContratacaoService.Domain.Common;
using ContratacaoService.Domain.Entities;
using ContratacaoService.Domain.Ports;

namespace ContratacaoService.Infrastructure.Repositories;

public class ContratacaoRepository : IContratacaoRepository
{
    private readonly ConcurrentDictionary<Guid, Contratacao> _contratacoes = new();
    private readonly ConcurrentDictionary<Guid, Guid> _propostaToContratacaoMap = new();

    public Task<Contratacao?> GetByIdAsync(Guid id)
    {
        _contratacoes.TryGetValue(id, out var contratacao);
        return Task.FromResult(contratacao);
    }

    public Task<PagedResult<Contratacao>> GetAllAsync(PagedRequest request)
    {
        var allContratacoes = _contratacoes.Values.ToList();
        var totalCount = allContratacoes.Count;
        
        var items = allContratacoes
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToList();

        return Task.FromResult(new PagedResult<Contratacao>
        {
            Items = items,
            TotalCount = totalCount,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize
        });
    }

    public Task<Contratacao?> GetByPropostaIdAsync(Guid propostaId)
    {
        if (_propostaToContratacaoMap.TryGetValue(propostaId, out var contratacaoId))
        {
            _contratacoes.TryGetValue(contratacaoId, out var contratacao);
            return Task.FromResult(contratacao);
        }
        
        return Task.FromResult<Contratacao?>(null);
    }

    public Task AddAsync(Contratacao contratacao)
    {
        if (contratacao == null)
            throw new ArgumentNullException(nameof(contratacao));

        if (!_contratacoes.TryAdd(contratacao.Id, contratacao))
            throw new InvalidOperationException($"Contratacao com ID {contratacao.Id} já existe");

        _propostaToContratacaoMap.TryAdd(contratacao.PropostaId, contratacao.Id);
        
        return Task.CompletedTask;
    }

    public Task DeleteAsync(Guid id)
    {
        if (_contratacoes.TryRemove(id, out var contratacao))
        {
            _propostaToContratacaoMap.TryRemove(contratacao.PropostaId, out _);
        }
        
        return Task.CompletedTask;
    }
}
