using PropostaService.Application.Commands;
using PropostaService.Application.DTOs;
using PropostaService.Domain.Entities;
using PropostaService.Domain.Ports;
using Shared.Messaging.Abstractions;
using Shared.Messaging.Events;

namespace PropostaService.Application.Handlers;

public class AlterarStatusPropostaCommandHandler : ICommandHandler<AlterarStatusPropostaCommand, PropostaResponse?>
{
    private readonly IPropostaRepository _repository;
    private readonly IEventPublisher _eventPublisher;

    public AlterarStatusPropostaCommandHandler(IPropostaRepository repository, IEventPublisher eventPublisher)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _eventPublisher = eventPublisher ?? throw new ArgumentNullException(nameof(eventPublisher));
    }

    public async Task<PropostaResponse?> HandleAsync(AlterarStatusPropostaCommand command)
    {
        var proposta = await _repository.GetByIdAsync(command.Id);
        if (proposta == null)
            return null;

        var statusAnterior = proposta.Status;
        proposta.AlterarStatus(command.NovoStatus);
        await _repository.UpdateAsync(proposta);

        var statusAlteradoEvent = new PropostaStatusAlteradaEvent(
            proposta.Id,
            proposta.NomeCliente.Valor,
            (int)statusAnterior,
            (int)proposta.Status,
            proposta.ValorCobertura.Valor,
            proposta.ValorPremio.Valor
        );

        await _eventPublisher.PublishAsync(statusAlteradoEvent);

        return MapToResponse(proposta);
    }

    private static PropostaResponse MapToResponse(Proposta proposta)
    {
        return new PropostaResponse(
            proposta.Id,
            proposta.NomeCliente.Valor,
            proposta.TipoCliente,
            proposta.TipoSeguro,
            proposta.ValorCobertura.Valor,
            proposta.ValorPremio.Valor,
            proposta.Status,
            proposta.DataCriacao
        );
    }
}
