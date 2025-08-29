using Shared.Saga.Models;

namespace Shared.Saga.Abstractions;

public interface ISagaOrchestrator
{
    Task<SagaResult> StartSagaAsync<TData>(string sagaType, TData data, string? correlationId = null, CancellationToken cancellationToken = default) where TData : class;
    Task<SagaResult> ResumeSagaAsync(Guid sagaId, CancellationToken cancellationToken = default);
    Task<SagaResult> CompensateSagaAsync(Guid sagaId, CancellationToken cancellationToken = default);
    Task<SagaInstance?> GetSagaAsync(Guid sagaId);
    Task<List<SagaInstance>> GetSagasByStatusAsync(SagaStatus status);
}
