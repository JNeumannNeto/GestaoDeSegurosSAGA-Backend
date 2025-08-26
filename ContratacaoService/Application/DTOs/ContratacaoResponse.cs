namespace ContratacaoService.Application.DTOs;

public record ContratacaoResponse(
    Guid Id,
    Guid PropostaId,
    DateTime DataContratacao,
    string NomeCliente,
    decimal ValorCobertura,
    decimal ValorPremio
);
