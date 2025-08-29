using ContratacaoService.Application.Sagas.ContratarPropostaSaga.Models;
using Microsoft.Extensions.Logging;
using Shared.Messaging.Abstractions;
using Shared.Messaging.Commands;
using Shared.Saga.Abstractions;

namespace ContratacaoService.Application.Sagas.ContratarPropostaSaga.Handlers;

public class ContratarPropostaSagaHandler : ICommandHandler<ContratarPropostaCommand>
{
    private readonly ISagaOrchestrator _sagaOrchestrator;
    private readonly ILogger<ContratarPropostaSagaHandler> _logger;

    public ContratarPropostaSagaHandler(
        ISagaOrchestrator sagaOrchestrator,
        ILogger<ContratarPropostaSagaHandler> logger)
    {
        _sagaOrchestrator = sagaOrchestrator ?? throw new ArgumentNullException(nameof(sagaOrchestrator));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task HandleAsync(ContratarPropostaCommand command)
    {
        _logger.LogInformation("Iniciando SAGA de contratação para proposta {PropostaId}", command.PropostaId);

        try
        {
            var sagaData = new ContratarPropostaData(command.PropostaId);
            var correlationId = $"contratacao-{command.PropostaId}";

            var result = await _sagaOrchestrator.StartSagaAsync(
                "ContratarProposta", 
                sagaData, 
                correlationId);

            if (result.IsSuccess)
            {
                _logger.LogInformation("SAGA de contratação iniciada com sucesso para proposta {PropostaId}", command.PropostaId);
            }
            else
            {
                _logger.LogError("SAGA de contratação falhou para proposta {PropostaId}: {ErrorMessage}", 
                    command.PropostaId, result.ErrorMessage);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro inesperado ao iniciar SAGA de contratação para proposta {PropostaId}", command.PropostaId);
            throw;
        }
    }
}
