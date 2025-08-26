using PropostaService.Application.DTOs;
using PropostaService.Application.Ports;
using PropostaService.Domain.Entities;
using PropostaService.Domain.Enums;
using PropostaService.Domain.Ports;
using Shared.Messaging.Abstractions;
using Shared.Messaging.Events;

namespace PropostaService.Application.Services;

public class PropostaAppService : IPropostaAppService
{
    private readonly IPropostaRepository _repository;
    private readonly IEventPublisher _eventPublisher;
    private readonly IValidationService _validationService;

    public PropostaAppService(IPropostaRepository repository, IEventPublisher eventPublisher, IValidationService validationService)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _eventPublisher = eventPublisher ?? throw new ArgumentNullException(nameof(eventPublisher));
        _validationService = validationService ?? throw new ArgumentNullException(nameof(validationService));
    }

    public async Task<PropostaResponse> CriarPropostaAsync(CriarPropostaRequest request)
    {
        var proposta = new Proposta(
            request.NomeCliente,
            request.TipoCliente,
            request.TipoSeguro,
            request.ValorCobertura,
            request.ValorPremio
        );

        await _repository.AddAsync(proposta);

        return MapToResponse(proposta);
    }

    public async Task<IEnumerable<PropostaResponse>> ListarPropostasAsync()
    {
        var propostas = await _repository.GetAllAsync();
        return propostas.Select(MapToResponse);
    }

    public async Task<PropostaResponse?> ObterPropostaPorIdAsync(Guid id)
    {
        var proposta = await _repository.GetByIdAsync(id);
        return proposta switch
        {
            null => null,
            _ => MapToResponse(proposta)
        };
    }

    public async Task<PropostaResponse?> AlterarStatusPropostaAsync(Guid id, AlterarStatusRequest request)
    {
        var proposta = await _repository.GetByIdAsync(id);
        return proposta switch
        {
            null => null,
            _ => await ProcessarAlteracaoStatus(proposta, request)
        };
    }

    private async Task<PropostaResponse> ProcessarAlteracaoStatus(Proposta proposta, AlterarStatusRequest request)
    {
        var statusAnterior = proposta.Status;
        proposta.AlterarStatus(request.NovoStatus);
        await _repository.UpdateAsync(proposta);

        await PublicarEventoStatusAlterado(proposta, statusAnterior);

        return MapToResponse(proposta);
    }

    private async Task PublicarEventoStatusAlterado(Proposta proposta, StatusProposta statusAnterior)
    {
        var statusAlteradoEvent = new PropostaStatusAlteradaEvent(
            proposta.Id,
            proposta.NomeCliente,
            (int)statusAnterior,
            (int)proposta.Status,
            proposta.ValorCobertura,
            proposta.ValorPremio
        );

        await _eventPublisher.PublishAsync(statusAlteradoEvent);
    }

    private static PropostaResponse MapToResponse(Proposta proposta)
    {
        return new PropostaResponse(
            proposta.Id,
            proposta.NomeCliente,
            proposta.TipoCliente,
            proposta.TipoSeguro,
            proposta.ValorCobertura,
            proposta.ValorPremio,
            proposta.Status,
            proposta.DataCriacao
        );
    }
}
