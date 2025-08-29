using System.Collections.Concurrent;
using Shared.Saga.Abstractions;
using Shared.Saga.Models;

namespace Shared.Saga.Storage;

public class InMemorySagaRepository : ISagaRepository
{
    private readonly ConcurrentDictionary<Guid, SagaInstance> _sagas = new();
    private readonly ConcurrentDictionary<string, Guid> _correlationIndex = new();

    public Task<SagaInstance?> GetByIdAsync(Guid sagaId)
    {
        _sagas.TryGetValue(sagaId, out var saga);
        return Task.FromResult(saga);
    }

    public Task<SagaInstance?> GetByCorrelationIdAsync(string correlationId)
    {
        if (_correlationIndex.TryGetValue(correlationId, out var sagaId))
        {
            return GetByIdAsync(sagaId);
        }
        return Task.FromResult<SagaInstance?>(null);
    }

    public Task<List<SagaInstance>> GetByStatusAsync(SagaStatus status)
    {
        var sagas = _sagas.Values.Where(s => s.Status == status).ToList();
        return Task.FromResult(sagas);
    }

    public Task SaveAsync(SagaInstance sagaInstance)
    {
        sagaInstance.CreatedAt = DateTime.UtcNow;
        sagaInstance.UpdatedAt = DateTime.UtcNow;
        
        _sagas.TryAdd(sagaInstance.Id, sagaInstance);
        
        if (!string.IsNullOrEmpty(sagaInstance.CorrelationId))
        {
            _correlationIndex.TryAdd(sagaInstance.CorrelationId, sagaInstance.Id);
        }
        
        return Task.CompletedTask;
    }

    public Task UpdateAsync(SagaInstance sagaInstance)
    {
        sagaInstance.UpdatedAt = DateTime.UtcNow;
        _sagas.TryUpdate(sagaInstance.Id, sagaInstance, _sagas[sagaInstance.Id]);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(Guid sagaId)
    {
        if (_sagas.TryRemove(sagaId, out var saga) && !string.IsNullOrEmpty(saga.CorrelationId))
        {
            _correlationIndex.TryRemove(saga.CorrelationId, out _);
        }
        return Task.CompletedTask;
    }
}
