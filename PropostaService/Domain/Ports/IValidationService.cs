using PropostaService.Domain.Enums;

namespace PropostaService.Domain.Ports;

public interface IValidationService
{
    bool IsTipoSeguroValidoParaTipoCliente(TipoCliente tipoCliente, TipoSeguro tipoSeguro);
    IEnumerable<TipoSeguro> ObterTiposSegurosPermitidos(TipoCliente tipoCliente);
    bool IsNomeClienteValido(string nomeCliente);
    bool IsValorCoberturaValido(decimal valorCobertura);
    bool IsValorPremioValido(decimal valorPremio);
}
