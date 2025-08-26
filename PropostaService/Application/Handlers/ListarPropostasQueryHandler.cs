using PropostaService.Application.DTOs;
using PropostaService.Application.Queries;
using PropostaService.Domain.Entities;
using PropostaService.Domain.Ports;

namespace PropostaService.Application.Handlers;

public class ListarPropostasQueryHandler : IQueryHandler<ListarPropostasQuery, IEnumerable<PropostaResponse>>
{
    private readonly IPropostaRepository _repository;

    public ListarPropostasQueryHandler(IPropostaRepository repository)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
    }

    public async Task<IEnumerable<PropostaResponse>> HandleAsync(ListarPropostasQuery query)
    {
        Console.WriteLine("[HANDLER] Iniciando busca de propostas...");
        var propostas = await _repository.GetAllAsync();
        Console.WriteLine($"[HANDLER] Propostas encontradas: {propostas.Count()}");
        
        var responses = propostas.Select(MapToResponse).ToList();
        Console.WriteLine($"[HANDLER] Responses criadas: {responses.Count}");
        
        return responses;
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
