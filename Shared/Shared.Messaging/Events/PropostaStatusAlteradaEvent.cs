namespace Shared.Messaging.Events;

public class PropostaStatusAlteradaEvent : BaseEvent
{
    public override string EventType => "PropostaStatusAlterada";
    
    public Guid PropostaId { get; private set; }
    public string NomeCliente { get; private set; }
    public int StatusAnterior { get; private set; }
    public int NovoStatus { get; private set; }
    public decimal ValorCobertura { get; private set; }
    public decimal ValorPremio { get; private set; }

    public PropostaStatusAlteradaEvent(
        Guid propostaId,
        string nomeCliente,
        int statusAnterior,
        int novoStatus,
        decimal valorCobertura,
        decimal valorPremio)
    {
        PropostaId = propostaId;
        NomeCliente = nomeCliente;
        StatusAnterior = statusAnterior;
        NovoStatus = novoStatus;
        ValorCobertura = valorCobertura;
        ValorPremio = valorPremio;
    }
}
