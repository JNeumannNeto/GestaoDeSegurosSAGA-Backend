using Shared.Saga.Models;

namespace Shared.Saga.Abstractions;

public interface ISagaRepository
{
    Task<SagaInstance?> GetByIdAsync(Guid sagaId);
    Task<SagaInstance?> GetByCorrelationIdAsync(string correlationId);
    Task<List<SagaInstance>> GetByStatusAsync(SagaStatus status);
    Task SaveAsync(SagaInstance sagaInstance);
    Task UpdateAsync(SagaInstance sagaInstance);
    Task DeleteAsync(Guid sagaId);
}
