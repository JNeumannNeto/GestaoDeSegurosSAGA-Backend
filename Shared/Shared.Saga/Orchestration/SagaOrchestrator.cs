using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Shared.Saga.Abstractions;
using Shared.Saga.Models;

namespace Shared.Saga.Orchestration;

public class SagaOrchestrator : ISagaOrchestrator
{
    private readonly ISagaRepository _sagaRepository;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<SagaOrchestrator> _logger;

    public SagaOrchestrator(
        ISagaRepository sagaRepository,
        IServiceProvider serviceProvider,
        ILogger<SagaOrchestrator> logger)
    {
        _sagaRepository = sagaRepository ?? throw new ArgumentNullException(nameof(sagaRepository));
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<SagaResult> StartSagaAsync<TData>(string sagaType, TData data, string? correlationId = null, CancellationToken cancellationToken = default) where TData : class
    {
        var sagaId = Guid.NewGuid();
        correlationId ??= sagaId.ToString();

        _logger.LogInformation("Starting saga {SagaType} with ID {SagaId} and correlation ID {CorrelationId}", sagaType, sagaId, correlationId);

        var sagaInstance = new SagaInstance
        {
            Id = sagaId,
            SagaType = sagaType,
            Status = SagaStatus.Running,
            CurrentStep = 0,
            LastCompletedStep = -1,
            CorrelationId = correlationId
        };

        sagaInstance.SetData(data);
        await _sagaRepository.SaveAsync(sagaInstance);

        return await ExecuteSagaAsync(sagaInstance, cancellationToken);
    }

    public async Task<SagaResult> ResumeSagaAsync(Guid sagaId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Resuming saga with ID {SagaId}", sagaId);

        var sagaInstance = await _sagaRepository.GetByIdAsync(sagaId);
        if (sagaInstance == null)
        {
            _logger.LogWarning("Saga with ID {SagaId} not found", sagaId);
            return SagaResult.Failed("Saga not found", -1);
        }

        return await ExecuteSagaAsync(sagaInstance, cancellationToken);
    }

    public async Task<SagaResult> CompensateSagaAsync(Guid sagaId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Compensating saga with ID {SagaId}", sagaId);

        var sagaInstance = await _sagaRepository.GetByIdAsync(sagaId);
        if (sagaInstance == null)
        {
            _logger.LogWarning("Saga with ID {SagaId} not found", sagaId);
            return SagaResult.Failed("Saga not found", -1);
        }

        sagaInstance.UpdateStatus(SagaStatus.Compensating);
        await _sagaRepository.UpdateAsync(sagaInstance);

        return await CompensateSagaStepsAsync(sagaInstance, cancellationToken);
    }

    public async Task<SagaInstance?> GetSagaAsync(Guid sagaId)
    {
        return await _sagaRepository.GetByIdAsync(sagaId);
    }

    public async Task<List<SagaInstance>> GetSagasByStatusAsync(SagaStatus status)
    {
        return await _sagaRepository.GetByStatusAsync(status);
    }

    private async Task<SagaResult> ExecuteSagaAsync(SagaInstance sagaInstance, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Executing saga {SagaType} with ID {SagaId}", sagaInstance.SagaType, sagaInstance.Id);
            
            sagaInstance.UpdateStatus(SagaStatus.Completed);
            await _sagaRepository.UpdateAsync(sagaInstance);

            _logger.LogInformation("Saga {SagaId} completed successfully", sagaInstance.Id);
            return SagaResult.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error executing saga {SagaId}", sagaInstance.Id);
            
            sagaInstance.UpdateStatus(SagaStatus.Failed, ex.Message);
            await _sagaRepository.UpdateAsync(sagaInstance);

            return SagaResult.Failed(ex.Message, sagaInstance.LastCompletedStep, ex);
        }
    }

    private async Task<SagaResult> CompensateSagaStepsAsync(SagaInstance sagaInstance, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Compensating saga {SagaId}", sagaInstance.Id);
            
            sagaInstance.UpdateStatus(SagaStatus.Compensated);
            await _sagaRepository.UpdateAsync(sagaInstance);

            _logger.LogInformation("Saga {SagaId} compensated successfully", sagaInstance.Id);
            return SagaResult.Compensated();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error compensating saga {SagaId}", sagaInstance.Id);
            
            sagaInstance.UpdateStatus(SagaStatus.CompensationFailed, ex.Message);
            await _sagaRepository.UpdateAsync(sagaInstance);

            return SagaResult.CompensationFailed(ex.Message, ex);
        }
    }
}
