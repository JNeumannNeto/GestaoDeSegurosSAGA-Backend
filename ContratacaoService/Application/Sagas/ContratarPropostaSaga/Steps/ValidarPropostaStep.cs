using ContratacaoService.Application.Sagas.ContratarPropostaSaga.Models;
using ContratacaoService.Domain.Ports;
using Microsoft.Extensions.Logging;
using Shared.Saga.Abstractions;
using Shared.Saga.Models;

namespace ContratacaoService.Application.Sagas.ContratarPropostaSaga.Steps;

public class ValidarPropostaStep : ISagaStep<ContratarPropostaData>
{
    private readonly IPropostaServiceClient _propostaServiceClient;
    private readonly ILogger<ValidarPropostaStep> _logger;

    public string StepName => "ValidarProposta";
    public int Order => 1;

    public ValidarPropostaStep(
        IPropostaServiceClient propostaServiceClient,
        ILogger<ValidarPropostaStep> logger)
    {
        _propostaServiceClient = propostaServiceClient ?? throw new ArgumentNullException(nameof(propostaServiceClient));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<StepResult> ExecuteAsync(ContratarPropostaData data, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Validando proposta {PropostaId}", data.PropostaId);

        try
        {
            var proposta = await _propostaServiceClient.ObterPropostaPorIdAsync(data.PropostaId);

            if (proposta == null)
            {
                var errorMessage = "Proposta não encontrada";
                _logger.LogWarning("Proposta {PropostaId} não encontrada", data.PropostaId);
                data.ErrorMessage = errorMessage;
                return StepResult.Failed(errorMessage);
            }

            data.Proposta = proposta;
            _logger.LogInformation("Proposta {PropostaId} validada com sucesso", data.PropostaId);
            
            return StepResult.Success();
        }
        catch (Exception ex)
        {
            var errorMessage = $"Erro ao validar proposta: {ex.Message}";
            _logger.LogError(ex, "Erro ao validar proposta {PropostaId}", data.PropostaId);
            data.ErrorMessage = errorMessage;
            return StepResult.Failed(errorMessage, ex);
        }
    }

    public Task<StepResult> CompensateAsync(ContratarPropostaData data, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Compensação não necessária para validação da proposta {PropostaId}", data.PropostaId);
        return Task.FromResult(StepResult.Success());
    }
}
