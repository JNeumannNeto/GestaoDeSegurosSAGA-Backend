using ContratacaoService.Domain.Entities;
using ContratacaoService.Domain.Models;

namespace ContratacaoService.Application.Sagas.ContratarPropostaSaga.Models;

public class ContratarPropostaData
{
    public Guid PropostaId { get; set; }
    public PropostaDto? Proposta { get; set; }
    public Contratacao? Contratacao { get; set; }
    public string? ErrorMessage { get; set; }
    public DateTime ProcessStartTime { get; set; } = DateTime.UtcNow;

    public ContratarPropostaData(Guid propostaId)
    {
        PropostaId = propostaId;
    }
}
