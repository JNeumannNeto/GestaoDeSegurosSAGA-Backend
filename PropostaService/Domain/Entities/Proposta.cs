using PropostaService.Domain.Common;
using PropostaService.Domain.Enums;
using PropostaService.Domain.Events;
using PropostaService.Domain.Services;
using PropostaService.Domain.ValueObjects;

namespace PropostaService.Domain.Entities;

public class Proposta : EntityBase
{
    public Guid Id { get; private set; }
    public NomeCliente NomeCliente { get; private set; }
    public TipoCliente TipoCliente { get; private set; }
    public TipoSeguro TipoSeguro { get; private set; }
    public ValorMonetario ValorCobertura { get; private set; }
    public ValorMonetario ValorPremio { get; private set; }
    public StatusProposta Status { get; private set; }
    public DateTime DataCriacao { get; private set; }

    public Proposta(string nomeCliente, TipoCliente tipoCliente, TipoSeguro tipoSeguro, decimal valorCobertura, decimal valorPremio)
    {
        ValidarCriacao(tipoCliente, tipoSeguro);

        Id = Guid.NewGuid();
        NomeCliente = new NomeCliente(nomeCliente);
        TipoCliente = tipoCliente;
        TipoSeguro = tipoSeguro;
        ValorCobertura = new ValorMonetario(valorCobertura);
        ValorPremio = new ValorMonetario(valorPremio);
        Status = StatusProposta.EmAnalise;
        DataCriacao = DateTime.UtcNow;

        AddDomainEvent(new PropostaCriadaEvent(
            Id,
            NomeCliente,
            TipoCliente,
            TipoSeguro,
            ValorCobertura,
            ValorPremio,
            DataCriacao
        ));
    }

    public void AlterarStatus(StatusProposta novoStatus)
    {
        if (Status == novoStatus)
            return;

        var statusAnterior = Status;
        Status = novoStatus;

        AddDomainEvent(new PropostaStatusAlteradoEvent(
            Id,
            statusAnterior,
            novoStatus,
            DateTime.UtcNow
        ));
    }

    public bool EstaAprovada() => Status == StatusProposta.Aprovada;
    public bool EstaRejeitada() => Status == StatusProposta.Rejeitada;
    public bool EstaEmAnalise() => Status == StatusProposta.EmAnalise;

    public bool PodeSerContratada() => EstaAprovada();

    public decimal CalcularComissao(decimal percentualComissao)
    {
        if (percentualComissao < 0 || percentualComissao > 100)
            throw new ArgumentException("Percentual de comissão deve estar entre 0 e 100", nameof(percentualComissao));

        return ValorPremio.Multiplicar(percentualComissao / 100);
    }

    public bool TemValorCoberturaMinima(decimal valorMinimo)
    {
        return ValorCobertura.EhMaiorQue(new ValorMonetario(valorMinimo)) || 
               ValorCobertura.EhIgualA(new ValorMonetario(valorMinimo));
    }

    private static void ValidarCriacao(TipoCliente tipoCliente, TipoSeguro tipoSeguro)
    {
        if (!PropostaDomainService.IsTipoSeguroValidoParaTipoCliente(tipoCliente, tipoSeguro))
            throw new ArgumentException($"Tipo de seguro {tipoSeguro} não é válido para {tipoCliente}");
    }
}
