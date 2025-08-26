using PropostaService.Application.Commands;
using PropostaService.Application.DTOs;
using PropostaService.Domain.Entities;
using PropostaService.Domain.Ports;
using PropostaService.Domain.Services;

namespace PropostaService.Application.Handlers;

public class CriarPropostaCommandHandler : ICommandHandler<CriarPropostaCommand, PropostaResponse>
{
    private readonly IPropostaRepository _repository;
    private readonly IValidationService _validationService;

    public CriarPropostaCommandHandler(IPropostaRepository repository, IValidationService validationService)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _validationService = validationService ?? throw new ArgumentNullException(nameof(validationService));
    }

    public async Task<PropostaResponse> HandleAsync(CriarPropostaCommand command)
    {
        var proposta = new Proposta(
            command.NomeCliente,
            command.TipoCliente,
            command.TipoSeguro,
            command.ValorCobertura,
            command.ValorPremio
        );

        await _repository.AddAsync(proposta);

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
