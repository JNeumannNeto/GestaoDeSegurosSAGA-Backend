using PropostaService.Domain.Enums;

namespace PropostaService.Domain.Events;

public record PropostaStatusAlteradoEvent(
    Guid PropostaId,
    StatusProposta StatusAnterior,
    StatusProposta NovoStatus,
    DateTime DataAlteracao
) : IDomainEvent;
