using PropostaService.Application.DTOs;
using PropostaService.Domain.Enums;

namespace PropostaService.Application.Commands;

public record AlterarStatusPropostaCommand(
    Guid Id,
    StatusProposta NovoStatus
) : ICommand<PropostaResponse?>;
