namespace Shared.Messaging.Commands;

public class ContratarPropostaCommand : ICommand
{
    public Guid Id { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public string CommandType => "ContratarProposta";
    
    public Guid PropostaId { get; private set; }

    public ContratarPropostaCommand(Guid propostaId)
    {
        Id = Guid.NewGuid();
        CreatedAt = DateTime.UtcNow;
        PropostaId = propostaId;
    }
}
