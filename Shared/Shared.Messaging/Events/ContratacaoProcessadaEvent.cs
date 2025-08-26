namespace Shared.Messaging.Events;

public class ContratacaoProcessadaEvent : BaseEvent
{
    public override string EventType => "ContratacaoProcessada";
    
    public Guid ContratacaoId { get; private set; }
    public Guid PropostaId { get; private set; }
    public string NomeCliente { get; private set; }
    public decimal ValorCobertura { get; private set; }
    public decimal ValorPremio { get; private set; }
    public DateTime DataContratacao { get; private set; }
    public bool Sucesso { get; private set; }
    public string? MensagemErro { get; private set; }

    public ContratacaoProcessadaEvent(
        Guid contratacaoId,
        Guid propostaId,
        string nomeCliente,
        decimal valorCobertura,
        decimal valorPremio,
        DateTime dataContratacao,
        bool sucesso,
        string? mensagemErro = null)
    {
        ContratacaoId = contratacaoId;
        PropostaId = propostaId;
        NomeCliente = nomeCliente;
        ValorCobertura = valorCobertura;
        ValorPremio = valorPremio;
        DataContratacao = dataContratacao;
        Sucesso = sucesso;
        MensagemErro = mensagemErro;
    }
}
