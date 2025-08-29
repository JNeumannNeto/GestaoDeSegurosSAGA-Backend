using Shared.Saga.Models;

namespace Shared.Saga.Abstractions;

public interface ISagaStep<TData> where TData : class
{
    string StepName { get; }
    int Order { get; }
    Task<StepResult> ExecuteAsync(TData data, CancellationToken cancellationToken = default);
    Task<StepResult> CompensateAsync(TData data, CancellationToken cancellationToken = default);
}
