using ContratacaoService.Application.Sagas.ContratarPropostaSaga.Models;
using Microsoft.Extensions.Logging;
using Shared.Messaging.Abstractions;
using Shared.Messaging.Events;
using Shared.Saga.Abstractions;
using Shared.Saga.Models;

namespace ContratacaoService.Application.Sagas.ContratarPropostaSaga.Steps;

public class PublicarEventoStep : ISagaStep<ContratarPropostaData>
{
    private readonly IEventPublisher _eventPublisher;
    private readonly ILogger<PublicarEventoStep> _logger;

    public string StepName => "PublicarEvento";
    public int Order => 4;

    public PublicarEventoStep(
        IEventPublisher eventPublisher,
        ILogger<PublicarEventoStep> logger)
    {
        _eventPublisher = eventPublisher ?? throw new ArgumentNullException(nameof(eventPublisher));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<StepResult> ExecuteAsync(ContratarPropostaData data, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Publicando evento de sucesso para contratação da proposta {PropostaId}", data.PropostaId);

        try
        {
            if (data.Contratacao == null)
            {
                var errorMessage = "Contratação não foi criada nas etapas anteriores";
                _logger.LogError(errorMessage);
                data.ErrorMessage = errorMessage;
                return StepResult.Failed(errorMessage);
            }

            var evento = new ContratacaoProcessadaEvent(
                data.Contratacao.Id,
                data.Contratacao.PropostaId,
                data.Contratacao.NomeCliente,
                data.Contratacao.ValorCobertura,
                data.Contratacao.ValorPremio,
                data.Contratacao.DataContratacao,
                true
            );

            await _eventPublisher.PublishAsync(evento);

            _logger.LogInformation("Evento de sucesso publicado para contratação {ContratacaoId}", data.Contratacao.Id);
            return StepResult.Success();
        }
        catch (Exception ex)
        {
            var errorMessage = $"Erro ao publicar evento de sucesso: {ex.Message}";
            _logger.LogError(ex, "Erro ao publicar evento para contratação da proposta {PropostaId}", data.PropostaId);
            data.ErrorMessage = errorMessage;
            return StepResult.Failed(errorMessage, ex);
        }
    }

    public async Task<StepResult> CompensateAsync(ContratarPropostaData data, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Publicando evento de falha para compensação da proposta {PropostaId}", data.PropostaId);

        try
        {
            var evento = new ContratacaoProcessadaEvent(
                Guid.Empty,
                data.PropostaId,
                data.Proposta?.NomeCliente ?? string.Empty,
                data.Proposta?.ValorCobertura ?? 0,
                data.Proposta?.ValorPremio ?? 0,
                DateTime.UtcNow,
                false,
                data.ErrorMessage ?? "Erro durante o processamento da SAGA"
            );

            await _eventPublisher.PublishAsync(evento);

            _logger.LogInformation("Evento de falha publicado para proposta {PropostaId}", data.PropostaId);
            return StepResult.Success();
        }
        catch (Exception ex)
        {
            var errorMessage = $"Erro ao publicar evento de falha: {ex.Message}";
            _logger.LogError(ex, "Erro ao publicar evento de falha para proposta {PropostaId}", data.PropostaId);
            return StepResult.Failed(errorMessage, ex);
        }
    }
}
