using PropostaService.Application.DTOs;
using PropostaService.Domain.Enums;

namespace PropostaService.Application.Commands;

public record CriarPropostaCommand(
    string NomeCliente,
    TipoCliente TipoCliente,
    TipoSeguro TipoSeguro,
    decimal ValorCobertura,
    decimal ValorPremio
) : ICommand<PropostaResponse>;

public interface ICommand<TResponse>
{
}
