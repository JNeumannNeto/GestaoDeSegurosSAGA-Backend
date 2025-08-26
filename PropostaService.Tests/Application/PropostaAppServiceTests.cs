using Moq;
using PropostaService.Application.DTOs;
using PropostaService.Application.Services;
using PropostaService.Domain.Entities;
using PropostaService.Domain.Enums;
using PropostaService.Domain.Ports;
using PropostaService.Domain.Services;
using Shared.Messaging.Abstractions;
using Shared.Messaging.Events;
using Xunit;

namespace PropostaService.Tests.Application;

public class PropostaAppServiceTests
{
    private readonly Mock<IPropostaRepository> _repositoryMock;
    private readonly Mock<IEventPublisher> _eventPublisherMock;
    private readonly Mock<IValidationService> _validationServiceMock;
    private readonly PropostaAppService _service;

    public PropostaAppServiceTests()
    {
        _repositoryMock = new Mock<IPropostaRepository>();
        _eventPublisherMock = new Mock<IEventPublisher>();
        _validationServiceMock = new Mock<IValidationService>();
        
        _validationServiceMock.Setup(v => v.IsTipoSeguroValidoParaTipoCliente(It.IsAny<TipoCliente>(), It.IsAny<TipoSeguro>()))
                             .Returns(true);
        _validationServiceMock.Setup(v => v.IsNomeClienteValido(It.IsAny<string>()))
                             .Returns(true);
        _validationServiceMock.Setup(v => v.IsValorCoberturaValido(It.IsAny<decimal>()))
                             .Returns(true);
        _validationServiceMock.Setup(v => v.IsValorPremioValido(It.IsAny<decimal>()))
                             .Returns(true);
        
        _service = new PropostaAppService(_repositoryMock.Object, _eventPublisherMock.Object, _validationServiceMock.Object);
    }

    [Fact]
    public async Task CriarPropostaAsync_ComDadosValidos_DeveCriarProposta()
    {
        var request = new CriarPropostaRequest(
            "João Silva",
            TipoCliente.PessoaFisica,
            TipoSeguro.Vida,
            100000m,
            500m
        );

        var result = await _service.CriarPropostaAsync(request);

        Assert.NotNull(result);
        Assert.Equal(request.NomeCliente, result.NomeCliente);
        Assert.Equal(request.TipoCliente, result.TipoCliente);
        Assert.Equal(request.TipoSeguro, result.TipoSeguro);
        Assert.Equal(request.ValorCobertura, result.ValorCobertura);
        Assert.Equal(request.ValorPremio, result.ValorPremio);
        Assert.Equal(StatusProposta.EmAnalise, result.Status);

        _repositoryMock.Verify(r => r.AddAsync(It.IsAny<Proposta>()), Times.Once);
    }

    [Fact]
    public async Task ObterPropostaPorIdAsync_ComIdExistente_DeveRetornarProposta()
    {
        var proposta = new Proposta("João Silva", TipoCliente.PessoaFisica, TipoSeguro.Vida, 100000m, 500m);
        _repositoryMock.Setup(r => r.GetByIdAsync(proposta.Id)).ReturnsAsync(proposta);

        var result = await _service.ObterPropostaPorIdAsync(proposta.Id);

        Assert.NotNull(result);
        Assert.Equal(proposta.Id, result.Id);
        Assert.Equal(proposta.NomeCliente.Valor, result.NomeCliente);
    }

    [Fact]
    public async Task ObterPropostaPorIdAsync_ComIdInexistente_DeveRetornarNull()
    {
        var id = Guid.NewGuid();
        _repositoryMock.Setup(r => r.GetByIdAsync(id)).ReturnsAsync((Proposta?)null);

        var result = await _service.ObterPropostaPorIdAsync(id);

        Assert.Null(result);
    }

    [Fact]
    public async Task ListarPropostasAsync_DeveRetornarTodasPropostas()
    {
        var propostas = new List<Proposta>
        {
            new("João Silva", TipoCliente.PessoaFisica, TipoSeguro.Vida, 100000m, 500m),
            new("Maria Santos", TipoCliente.PessoaFisica, TipoSeguro.Automovel, 50000m, 800m)
        };
        _repositoryMock.Setup(r => r.GetAllAsync()).ReturnsAsync(propostas);

        var result = await _service.ListarPropostasAsync();

        Assert.NotNull(result);
        Assert.Equal(2, result.Count());
    }

    [Fact]
    public async Task AlterarStatusPropostaAsync_ComIdExistente_DeveAlterarStatusEPublicarEvento()
    {
        var proposta = new Proposta("João Silva", TipoCliente.PessoaFisica, TipoSeguro.Vida, 100000m, 500m);
        var request = new AlterarStatusRequest(StatusProposta.Aprovada);
        
        _repositoryMock.Setup(r => r.GetByIdAsync(proposta.Id)).ReturnsAsync(proposta);

        var result = await _service.AlterarStatusPropostaAsync(proposta.Id, request);

        Assert.NotNull(result);
        Assert.Equal(StatusProposta.Aprovada, result.Status);

        _repositoryMock.Verify(r => r.UpdateAsync(It.IsAny<Proposta>()), Times.Once);
        _eventPublisherMock.Verify(e => e.PublishAsync(It.IsAny<PropostaStatusAlteradaEvent>()), Times.Once);
    }

    [Fact]
    public async Task AlterarStatusPropostaAsync_ComIdInexistente_DeveRetornarNull()
    {
        var id = Guid.NewGuid();
        var request = new AlterarStatusRequest(StatusProposta.Aprovada);
        
        _repositoryMock.Setup(r => r.GetByIdAsync(id)).ReturnsAsync((Proposta?)null);

        var result = await _service.AlterarStatusPropostaAsync(id, request);

        Assert.Null(result);
        _repositoryMock.Verify(r => r.UpdateAsync(It.IsAny<Proposta>()), Times.Never);
        _eventPublisherMock.Verify(e => e.PublishAsync(It.IsAny<PropostaStatusAlteradaEvent>()), Times.Never);
    }
}
