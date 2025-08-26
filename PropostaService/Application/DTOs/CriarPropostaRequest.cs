using PropostaService.Domain.Enums;

namespace PropostaService.Application.DTOs;

public record CriarPropostaRequest(
    string NomeCliente,
    TipoCliente TipoCliente,
    TipoSeguro TipoSeguro,
    decimal ValorCobertura,
    decimal ValorPremio
);
