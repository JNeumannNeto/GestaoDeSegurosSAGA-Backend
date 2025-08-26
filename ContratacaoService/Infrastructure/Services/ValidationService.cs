using ContratacaoService.Domain.Ports;

namespace ContratacaoService.Infrastructure.Services;

public class ValidationService : IValidationService
{
    public bool IsNomeClienteValido(string nomeCliente)
    {
        return !string.IsNullOrWhiteSpace(nomeCliente);
    }

    public bool IsValorCoberturaValido(decimal valorCobertura)
    {
        return valorCobertura > 0;
    }

    public bool IsValorPremioValido(decimal valorPremio)
    {
        return valorPremio > 0;
    }

    public bool IsPropostaIdValido(Guid propostaId)
    {
        return propostaId != Guid.Empty;
    }
}
