using PropostaService.Domain.Enums;
using PropostaService.Domain.Ports;
using PropostaService.Domain.Services;

namespace PropostaService.Infrastructure.Services;

public class ValidationService : IValidationService
{
    public bool IsTipoSeguroValidoParaTipoCliente(TipoCliente tipoCliente, TipoSeguro tipoSeguro)
    {
        return PropostaDomainService.IsTipoSeguroValidoParaTipoCliente(tipoCliente, tipoSeguro);
    }

    public IEnumerable<TipoSeguro> ObterTiposSegurosPermitidos(TipoCliente tipoCliente)
    {
        return PropostaDomainService.ObterTiposSegurosPermitidos(tipoCliente);
    }

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
}
