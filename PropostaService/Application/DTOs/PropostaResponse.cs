using PropostaService.Domain.Enums;

namespace PropostaService.Application.DTOs;

public record PropostaResponse(
    Guid Id,
    string NomeCliente,
    TipoCliente TipoCliente,
    TipoSeguro TipoSeguro,
    decimal ValorCobertura,
    decimal ValorPremio,
    StatusProposta Status,
    DateTime DataCriacao
);
