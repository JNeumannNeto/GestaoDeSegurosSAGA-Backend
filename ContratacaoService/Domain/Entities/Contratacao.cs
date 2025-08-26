using ContratacaoService.Domain.Ports;

namespace ContratacaoService.Domain.Entities;

public class Contratacao
{
    public Guid Id { get; private set; }
    public Guid PropostaId { get; private set; }
    public DateTime DataContratacao { get; private set; }
    public string NomeCliente { get; private set; }
    public decimal ValorCobertura { get; private set; }
    public decimal ValorPremio { get; private set; }

    public Contratacao(Guid propostaId, string nomeCliente, decimal valorCobertura, decimal valorPremio, IValidationService validationService)
    {
        if (!validationService.IsPropostaIdValido(propostaId))
            throw new ArgumentException("ID da proposta é obrigatório", nameof(propostaId));
        
        if (!validationService.IsNomeClienteValido(nomeCliente))
            throw new ArgumentException("Nome do cliente é obrigatório", nameof(nomeCliente));
        
        if (!validationService.IsValorCoberturaValido(valorCobertura))
            throw new ArgumentException("Valor de cobertura deve ser maior que zero", nameof(valorCobertura));
        
        if (!validationService.IsValorPremioValido(valorPremio))
            throw new ArgumentException("Valor do prêmio deve ser maior que zero", nameof(valorPremio));

        Id = Guid.NewGuid();
        PropostaId = propostaId;
        DataContratacao = DateTime.UtcNow;
        NomeCliente = nomeCliente;
        ValorCobertura = valorCobertura;
        ValorPremio = valorPremio;
    }
}
