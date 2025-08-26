using PropostaService.Domain.Enums;

namespace PropostaService.Application.DTOs;

public record AlterarStatusRequest(
    StatusProposta NovoStatus
);
