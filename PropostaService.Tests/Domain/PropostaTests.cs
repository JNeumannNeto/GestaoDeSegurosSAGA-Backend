using PropostaService.Domain.Entities;
using PropostaService.Domain.Enums;
using PropostaService.Domain.Events;
using Xunit;

namespace PropostaService.Tests.Domain;

public class PropostaTests
{
    [Fact]
    public void CriarProposta_ComDadosValidos_DeveRetornarPropostaComStatusEmAnalise()
    {
        var proposta = new Proposta(
            "João Silva",
            TipoCliente.PessoaFisica,
            TipoSeguro.Vida,
            100000m,
            500m
        );

        Assert.NotEqual(Guid.Empty, proposta.Id);
        Assert.Equal("João Silva", proposta.NomeCliente.Valor);
        Assert.Equal(TipoCliente.PessoaFisica, proposta.TipoCliente);
        Assert.Equal(TipoSeguro.Vida, proposta.TipoSeguro);
        Assert.Equal(100000m, proposta.ValorCobertura.Valor);
        Assert.Equal(500m, proposta.ValorPremio.Valor);
        Assert.Equal(StatusProposta.EmAnalise, proposta.Status);
        Assert.True(proposta.DataCriacao <= DateTime.UtcNow);
    }

    [Fact]
    public void CriarProposta_ComDadosValidos_DeveGerarEventoDeCriacao()
    {
        var proposta = new Proposta(
            "João Silva",
            TipoCliente.PessoaFisica,
            TipoSeguro.Vida,
            100000m,
            500m
        );

        Assert.Single(proposta.DomainEvents);
        var evento = proposta.DomainEvents.First() as PropostaCriadaEvent;
        Assert.NotNull(evento);
        Assert.Equal(proposta.Id, evento.PropostaId);
        Assert.Equal("João Silva", evento.NomeCliente.Valor);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void CriarProposta_ComNomeClienteInvalido_DeveLancarExcecao(string nomeCliente)
    {
        var exception = Assert.Throws<ArgumentException>(() =>
            new Proposta(nomeCliente, TipoCliente.PessoaFisica, TipoSeguro.Vida, 100000m, 500m));

        Assert.Contains("Nome do cliente não pode ser vazio", exception.Message);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1000)]
    public void CriarProposta_ComValorCoberturaInvalido_DeveLancarExcecao(decimal valorCobertura)
    {
        var exception = Assert.Throws<ArgumentException>(() =>
            new Proposta("João Silva", TipoCliente.PessoaFisica, TipoSeguro.Vida, valorCobertura, 500m));

        Assert.Contains("Valor monetário deve ser maior que zero", exception.Message);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-500)]
    public void CriarProposta_ComValorPremioInvalido_DeveLancarExcecao(decimal valorPremio)
    {
        var exception = Assert.Throws<ArgumentException>(() =>
            new Proposta("João Silva", TipoCliente.PessoaFisica, TipoSeguro.Vida, 100000m, valorPremio));

        Assert.Contains("Valor monetário deve ser maior que zero", exception.Message);
    }

    [Fact]
    public void CriarProposta_PessoaFisicaComSeguroEmpresarial_DeveLancarExcecao()
    {
        var exception = Assert.Throws<ArgumentException>(() =>
            new Proposta("João Silva", TipoCliente.PessoaFisica, TipoSeguro.Empresarial, 100000m, 500m));

        Assert.Contains("Tipo de seguro Empresarial não é válido para PessoaFisica", exception.Message);
    }

    [Fact]
    public void CriarProposta_PessoaJuridicaComSeguroVida_DeveLancarExcecao()
    {
        var exception = Assert.Throws<ArgumentException>(() =>
            new Proposta("Empresa ABC", TipoCliente.PessoaJuridica, TipoSeguro.Vida, 100000m, 500m));

        Assert.Contains("Tipo de seguro Vida não é válido para PessoaJuridica", exception.Message);
    }

    [Fact]
    public void AlterarStatus_ComNovoStatus_DeveAlterarStatusEGerarEvento()
    {
        var proposta = new Proposta("João Silva", TipoCliente.PessoaFisica, TipoSeguro.Vida, 100000m, 500m);
        proposta.ClearDomainEvents();

        proposta.AlterarStatus(StatusProposta.Aprovada);

        Assert.Equal(StatusProposta.Aprovada, proposta.Status);
        Assert.Single(proposta.DomainEvents);
        var evento = proposta.DomainEvents.First() as PropostaStatusAlteradoEvent;
        Assert.NotNull(evento);
        Assert.Equal(StatusProposta.EmAnalise, evento.StatusAnterior);
        Assert.Equal(StatusProposta.Aprovada, evento.NovoStatus);
    }

    [Fact]
    public void AlterarStatus_ComMesmoStatus_NaoDeveAlterarStatusNemGerarEvento()
    {
        var proposta = new Proposta("João Silva", TipoCliente.PessoaFisica, TipoSeguro.Vida, 100000m, 500m);
        proposta.ClearDomainEvents();
        var statusInicial = proposta.Status;

        proposta.AlterarStatus(StatusProposta.EmAnalise);

        Assert.Equal(statusInicial, proposta.Status);
        Assert.Empty(proposta.DomainEvents);
    }

    [Fact]
    public void EstaAprovada_ComStatusAprovada_DeveRetornarTrue()
    {
        var proposta = new Proposta("João Silva", TipoCliente.PessoaFisica, TipoSeguro.Vida, 100000m, 500m);
        proposta.AlterarStatus(StatusProposta.Aprovada);

        Assert.True(proposta.EstaAprovada());
        Assert.True(proposta.PodeSerContratada());
    }

    [Fact]
    public void CalcularComissao_ComPercentualValido_DeveRetornarValorCorreto()
    {
        var proposta = new Proposta("João Silva", TipoCliente.PessoaFisica, TipoSeguro.Vida, 100000m, 1000m);

        var comissao = proposta.CalcularComissao(10m);

        Assert.Equal(100m, comissao);
    }

    [Fact]
    public void TemValorCoberturaMinima_ComValorSuficiente_DeveRetornarTrue()
    {
        var proposta = new Proposta("João Silva", TipoCliente.PessoaFisica, TipoSeguro.Vida, 100000m, 500m);

        Assert.True(proposta.TemValorCoberturaMinima(50000m));
        Assert.True(proposta.TemValorCoberturaMinima(100000m));
        Assert.False(proposta.TemValorCoberturaMinima(150000m));
    }
}
