using PropostaService.Domain.Enums;
using PropostaService.Domain.ValueObjects;

namespace PropostaService.Domain.Events;

public record PropostaCriadaEvent(
    Guid PropostaId,
    NomeCliente NomeCliente,
    TipoCliente TipoCliente,
    TipoSeguro TipoSeguro,
    ValorMonetario ValorCobertura,
    ValorMonetario ValorPremio,
    DateTime DataCriacao
) : IDomainEvent;
