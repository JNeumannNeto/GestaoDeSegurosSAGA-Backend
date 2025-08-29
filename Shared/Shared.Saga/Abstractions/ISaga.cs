using Shared.Saga.Models;

namespace Shared.Saga.Abstractions;

public interface ISaga<TData> where TData : class
{
    string SagaType { get; }
    List<ISagaStep<TData>> Steps { get; }
    Task<SagaResult> ExecuteAsync(TData data, CancellationToken cancellationToken = default);
    Task<SagaResult> CompensateAsync(TData data, int lastCompletedStep, CancellationToken cancellationToken = default);
}
