namespace ContratacaoService.Domain.Ports;

public interface IValidationService
{
    bool IsNomeClienteValido(string nomeCliente);
    bool IsValorCoberturaValido(decimal valorCobertura);
    bool IsValorPremioValido(decimal valorPremio);
    bool IsPropostaIdValido(Guid propostaId);
}
