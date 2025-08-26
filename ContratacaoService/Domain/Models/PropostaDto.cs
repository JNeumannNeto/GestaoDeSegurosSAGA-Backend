namespace ContratacaoService.Domain.Models;

public record PropostaDto(
    Guid Id,
    string NomeCliente,
    int TipoCliente,
    int TipoSeguro,
    decimal ValorCobertura,
    decimal ValorPremio,
    int Status,
    DateTime DataCriacao
);
