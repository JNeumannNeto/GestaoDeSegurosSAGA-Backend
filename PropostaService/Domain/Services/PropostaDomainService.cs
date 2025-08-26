using PropostaService.Domain.Enums;

namespace PropostaService.Domain.Services;

public static class PropostaDomainService
{
    private static readonly Dictionary<TipoCliente, TipoSeguro[]> _regrasValidacao = new()
    {
        {
            TipoCliente.PessoaFisica,
            new[] { TipoSeguro.Vida, TipoSeguro.Saude, TipoSeguro.Automovel, TipoSeguro.Residencial }
        },
        {
            TipoCliente.PessoaJuridica,
            new[] { TipoSeguro.Empresarial, TipoSeguro.Cargas, TipoSeguro.Frota, TipoSeguro.Condominio, TipoSeguro.VidaEmpresarial }
        }
    };

    public static bool IsTipoSeguroValidoParaTipoCliente(TipoCliente tipoCliente, TipoSeguro tipoSeguro)
    {
        return _regrasValidacao.ContainsKey(tipoCliente) && 
               _regrasValidacao[tipoCliente].Contains(tipoSeguro);
    }

    public static IEnumerable<TipoSeguro> ObterTiposSegurosPermitidos(TipoCliente tipoCliente)
    {
        return _regrasValidacao.ContainsKey(tipoCliente) 
            ? _regrasValidacao[tipoCliente] 
            : Enumerable.Empty<TipoSeguro>();
    }
}
