using PropostaService.Application.DTOs;
using PropostaService.Application.Queries;
using PropostaService.Domain.Entities;
using PropostaService.Domain.Ports;

namespace PropostaService.Application.Handlers;

public class ObterPropostaPorIdQueryHandler : IQueryHandler<ObterPropostaPorIdQuery, PropostaResponse?>
{
    private readonly IPropostaRepository _repository;

    public ObterPropostaPorIdQueryHandler(IPropostaRepository repository)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
    }

    public async Task<PropostaResponse?> HandleAsync(ObterPropostaPorIdQuery query)
    {
        var proposta = await _repository.GetByIdAsync(query.Id);
        return proposta != null ? MapToResponse(proposta) : null;
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
