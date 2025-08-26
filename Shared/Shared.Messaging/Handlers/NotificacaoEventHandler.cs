using Microsoft.Extensions.Logging;
using Shared.Messaging.Abstractions;
using Shared.Messaging.Events;

namespace Shared.Messaging.Handlers;

public class NotificacaoEventHandler : 
    IEventHandler<PropostaStatusAlteradaEvent>,
    IEventHandler<ContratacaoProcessadaEvent>
{
    private readonly ILogger<NotificacaoEventHandler> _logger;

    public NotificacaoEventHandler(ILogger<NotificacaoEventHandler> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task HandleAsync(PropostaStatusAlteradaEvent @event)
    {
        var statusTexto = @event.NovoStatus switch
        {
            0 => "Em Análise",
            1 => "Aprovada",
            2 => "Rejeitada",
            _ => "Desconhecido"
        };

        _logger.LogInformation(
            "NOTIFICAÇÃO: Proposta {PropostaId} do cliente {NomeCliente} teve status alterado para {Status}",
            @event.PropostaId,
            @event.NomeCliente,
            statusTexto);

        await Task.CompletedTask;
    }

    public async Task HandleAsync(ContratacaoProcessadaEvent @event)
    {
        if (@event.Sucesso)
        {
            _logger.LogInformation(
                "NOTIFICAÇÃO: Contratação {ContratacaoId} processada com sucesso para o cliente {NomeCliente}. Valor da cobertura: {ValorCobertura:C}",
                @event.ContratacaoId,
                @event.NomeCliente,
                @event.ValorCobertura);
        }
        else
        {
            _logger.LogWarning(
                "NOTIFICAÇÃO: Falha no processamento da contratação da proposta {PropostaId}. Erro: {MensagemErro}",
                @event.PropostaId,
                @event.MensagemErro);
        }

        await Task.CompletedTask;
    }
}
