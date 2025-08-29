using ContratacaoService.Application.Sagas.ContratarPropostaSaga.Models;
using ContratacaoService.Application.Sagas.ContratarPropostaSaga.Steps;
using Microsoft.Extensions.Logging;
using Shared.Saga.Abstractions;
using Shared.Saga.Models;

namespace ContratacaoService.Application.Sagas.ContratarPropostaSaga;

public class ContratarPropostaSaga : ISaga<ContratarPropostaData>
{
    private readonly ILogger<ContratarPropostaSaga> _logger;

    public string SagaType => "ContratarProposta";

    public List<ISagaStep<ContratarPropostaData>> Steps { get; }

    public ContratarPropostaSaga(
        ValidarPropostaStep validarPropostaStep,
        VerificarDisponibilidadeStep verificarDisponibilidadeStep,
        CriarContratacaoStep criarContratacaoStep,
        PublicarEventoStep publicarEventoStep,
        ILogger<ContratarPropostaSaga> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        Steps = new List<ISagaStep<ContratarPropostaData>>
        {
            validarPropostaStep,
            verificarDisponibilidadeStep,
            criarContratacaoStep,
            publicarEventoStep
        };
    }

    public async Task<SagaResult> ExecuteAsync(ContratarPropostaData data, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Iniciando execução da SAGA ContratarProposta para proposta {PropostaId}", data.PropostaId);

        var orderedSteps = Steps.OrderBy(s => s.Order).ToList();

        for (int i = 0; i < orderedSteps.Count; i++)
        {
            var step = orderedSteps[i];
            _logger.LogInformation("Executando step {StepName} ({Order}) da SAGA ContratarProposta", step.StepName, step.Order);

            try
            {
                var stepResult = await step.ExecuteAsync(data, cancellationToken);

                if (!stepResult.IsSuccess)
                {
                    _logger.LogError("Step {StepName} falhou: {ErrorMessage}", step.StepName, stepResult.ErrorMessage);
                    
                    var compensationResult = await CompensateAsync(data, i - 1, cancellationToken);
                    return compensationResult.IsSuccess 
                        ? SagaResult.Failed(stepResult.ErrorMessage!, i - 1)
                        : SagaResult.CompensationFailed(compensationResult.ErrorMessage!, compensationResult.Exception);
                }

                _logger.LogInformation("Step {StepName} executado com sucesso", step.StepName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro inesperado no step {StepName}", step.StepName);
                
                var compensationResult = await CompensateAsync(data, i - 1, cancellationToken);
                return compensationResult.IsSuccess 
                    ? SagaResult.Failed(ex.Message, i - 1, ex)
                    : SagaResult.CompensationFailed(compensationResult.ErrorMessage!, compensationResult.Exception);
            }
        }

        _logger.LogInformation("SAGA ContratarProposta executada com sucesso para proposta {PropostaId}", data.PropostaId);
        return SagaResult.Success();
    }

    public async Task<SagaResult> CompensateAsync(ContratarPropostaData data, int lastCompletedStep, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Iniciando compensação da SAGA ContratarProposta para proposta {PropostaId} a partir do step {LastStep}", 
            data.PropostaId, lastCompletedStep);

        var orderedSteps = Steps.OrderByDescending(s => s.Order).ToList();

        for (int i = lastCompletedStep; i >= 0; i--)
        {
            var step = orderedSteps.FirstOrDefault(s => s.Order == i);
            if (step == null) continue;

            _logger.LogInformation("Compensando step {StepName} ({Order}) da SAGA ContratarProposta", step.StepName, step.Order);

            try
            {
                var compensationResult = await step.CompensateAsync(data, cancellationToken);

                if (!compensationResult.IsSuccess)
                {
                    _logger.LogError("Compensação do step {StepName} falhou: {ErrorMessage}", step.StepName, compensationResult.ErrorMessage);
                    return SagaResult.CompensationFailed(compensationResult.ErrorMessage!, compensationResult.Exception);
                }

                _logger.LogInformation("Step {StepName} compensado com sucesso", step.StepName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro inesperado na compensação do step {StepName}", step.StepName);
                return SagaResult.CompensationFailed(ex.Message, ex);
            }
        }

        _logger.LogInformation("Compensação da SAGA ContratarProposta concluída para proposta {PropostaId}", data.PropostaId);
        return SagaResult.Compensated();
    }
}
