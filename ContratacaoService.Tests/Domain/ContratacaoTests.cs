using ContratacaoService.Domain.Entities;
using ContratacaoService.Domain.Ports;
using Moq;
using Xunit;

namespace ContratacaoService.Tests.Domain;

public class ContratacaoTests
{
    private readonly Mock<IValidationService> _mockValidationService;

    public ContratacaoTests()
    {
        _mockValidationService = new Mock<IValidationService>();
        _mockValidationService.Setup(x => x.IsPropostaIdValido(It.IsAny<Guid>())).Returns(true);
        _mockValidationService.Setup(x => x.IsNomeClienteValido(It.IsAny<string>())).Returns(true);
        _mockValidationService.Setup(x => x.IsValorCoberturaValido(It.IsAny<decimal>())).Returns(true);
        _mockValidationService.Setup(x => x.IsValorPremioValido(It.IsAny<decimal>())).Returns(true);
    }

    [Fact]
    public void CriarContratacao_ComDadosValidos_DeveRetornarContratacaoValida()
    {
        var propostaId = Guid.NewGuid();
        var nomeCliente = "João Silva";
        var valorCobertura = 100000m;
        var valorPremio = 500m;

        var contratacao = new Contratacao(propostaId, nomeCliente, valorCobertura, valorPremio, _mockValidationService.Object);

        Assert.NotEqual(Guid.Empty, contratacao.Id);
        Assert.Equal(propostaId, contratacao.PropostaId);
        Assert.Equal(nomeCliente, contratacao.NomeCliente);
        Assert.Equal(valorCobertura, contratacao.ValorCobertura);
        Assert.Equal(valorPremio, contratacao.ValorPremio);
        Assert.True(contratacao.DataContratacao <= DateTime.UtcNow);
    }

    [Fact]
    public void CriarContratacao_ComPropostaIdVazio_DeveLancarExcecao()
    {
        _mockValidationService.Setup(x => x.IsPropostaIdValido(Guid.Empty)).Returns(false);

        var exception = Assert.Throws<ArgumentException>(() =>
            new Contratacao(Guid.Empty, "João Silva", 100000m, 500m, _mockValidationService.Object));

        Assert.Equal("ID da proposta é obrigatório (Parameter 'propostaId')", exception.Message);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void CriarContratacao_ComNomeClienteInvalido_DeveLancarExcecao(string nomeCliente)
    {
        _mockValidationService.Setup(x => x.IsNomeClienteValido(nomeCliente)).Returns(false);

        var exception = Assert.Throws<ArgumentException>(() =>
            new Contratacao(Guid.NewGuid(), nomeCliente, 100000m, 500m, _mockValidationService.Object));

        Assert.Equal("Nome do cliente é obrigatório (Parameter 'nomeCliente')", exception.Message);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1000)]
    public void CriarContratacao_ComValorCoberturaInvalido_DeveLancarExcecao(decimal valorCobertura)
    {
        _mockValidationService.Setup(x => x.IsValorCoberturaValido(valorCobertura)).Returns(false);

        var exception = Assert.Throws<ArgumentException>(() =>
            new Contratacao(Guid.NewGuid(), "João Silva", valorCobertura, 500m, _mockValidationService.Object));

        Assert.Equal("Valor de cobertura deve ser maior que zero (Parameter 'valorCobertura')", exception.Message);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-500)]
    public void CriarContratacao_ComValorPremioInvalido_DeveLancarExcecao(decimal valorPremio)
    {
        _mockValidationService.Setup(x => x.IsValorPremioValido(valorPremio)).Returns(false);

        var exception = Assert.Throws<ArgumentException>(() =>
            new Contratacao(Guid.NewGuid(), "João Silva", 100000m, valorPremio, _mockValidationService.Object));

        Assert.Equal("Valor do prêmio deve ser maior que zero (Parameter 'valorPremio')", exception.Message);
    }
}
