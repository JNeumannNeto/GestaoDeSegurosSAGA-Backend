using ContratacaoService.Application.Sagas.ContratarPropostaSaga.Models;
using ContratacaoService.Domain.Enums;
using ContratacaoService.Domain.Ports;
using Microsoft.Extensions.Logging;
using Shared.Saga.Abstractions;
using Shared.Saga.Models;

namespace ContratacaoService.Application.Sagas.ContratarPropostaSaga.Steps;

public class VerificarDisponibilidadeStep : ISagaStep<ContratarPropostaData>
{
    private readonly IContratacaoRepository _contratacaoRepository;
    private readonly ILogger<VerificarDisponibilidadeStep> _logger;

    public string StepName => "VerificarDisponibilidade";
    public int Order => 2;

    public VerificarDisponibilidadeStep(
        IContratacaoRepository contratacaoRepository,
        ILogger<VerificarDisponibilidadeStep> logger)
    {
        _contratacaoRepository = contratacaoRepository ?? throw new ArgumentNullException(nameof(contratacaoRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<StepResult> ExecuteAsync(ContratarPropostaData data, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Verificando disponibilidade da proposta {PropostaId}", data.PropostaId);

        try
        {
            if (data.Proposta == null)
            {
                var errorMessage = "Proposta não foi carregada na etapa anterior";
                _logger.LogError(errorMessage);
                data.ErrorMessage = errorMessage;
                return StepResult.Failed(errorMessage);
            }

            if (data.Proposta.Status != (int)StatusProposta.Aprovada)
            {
                var errorMessage = "Apenas propostas aprovadas podem ser contratadas";
                _logger.LogWarning("Proposta {PropostaId} não está aprovada. Status atual: {Status}", data.PropostaId, data.Proposta.Status);
                data.ErrorMessage = errorMessage;
                return StepResult.Failed(errorMessage);
            }

            var propostaJaContratada = await _contratacaoRepository.GetByPropostaIdAsync(data.PropostaId);
            if (propostaJaContratada != null)
            {
                var errorMessage = "Proposta já foi contratada anteriormente";
                _logger.LogWarning("Proposta {PropostaId} já foi contratada", data.PropostaId);
                data.ErrorMessage = errorMessage;
                return StepResult.Failed(errorMessage);
            }

            _logger.LogInformation("Proposta {PropostaId} está disponível para contratação", data.PropostaId);
            return StepResult.Success();
        }
        catch (Exception ex)
        {
            var errorMessage = $"Erro ao verificar disponibilidade da proposta: {ex.Message}";
            _logger.LogError(ex, "Erro ao verificar disponibilidade da proposta {PropostaId}", data.PropostaId);
            data.ErrorMessage = errorMessage;
            return StepResult.Failed(errorMessage, ex);
        }
    }

    public Task<StepResult> CompensateAsync(ContratarPropostaData data, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Compensação não necessária para verificação de disponibilidade da proposta {PropostaId}", data.PropostaId);
        return Task.FromResult(StepResult.Success());
    }
}
