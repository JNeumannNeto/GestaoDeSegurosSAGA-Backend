using ContratacaoService.Application.DTOs;
using ContratacaoService.Application.Services;
using ContratacaoService.Domain.Common;
using ContratacaoService.Domain.Entities;
using ContratacaoService.Domain.Ports;
using Moq;
using Shared.Messaging.Abstractions;
using Shared.Messaging.Commands;
using Xunit;

namespace ContratacaoService.Tests.Application;

public class ContratacaoAppServiceTests
{
    private readonly Mock<IContratacaoRepository> _mockRepository;
    private readonly Mock<ICommandPublisher> _mockCommandPublisher;
    private readonly Mock<IValidationService> _mockValidationService;
    private readonly ContratacaoAppService _appService;

    public ContratacaoAppServiceTests()
    {
        _mockRepository = new Mock<IContratacaoRepository>();
        _mockCommandPublisher = new Mock<ICommandPublisher>();
        _mockValidationService = new Mock<IValidationService>();
        _appService = new ContratacaoAppService(_mockRepository.Object, _mockCommandPublisher.Object);
    }

    [Fact]
    public async Task ContratarPropostaAsync_ComPropostaIdValido_DevePublicarComando()
    {
        var propostaId = Guid.NewGuid();
        var request = new ContratarPropostaRequest(propostaId);

        var result = await _appService.ContratarPropostaAsync(request);

        Assert.True(result.IsSuccess);
        Assert.Contains(propostaId.ToString(), result.Value);
        _mockCommandPublisher.Verify(x => x.PublishAsync(It.Is<ContratarPropostaCommand>(c => c.PropostaId == propostaId)), Times.Once);
    }

    [Fact]
    public async Task ContratarPropostaAsync_ComPropostaIdVazio_DeveRetornarErro()
    {
        var request = new ContratarPropostaRequest(Guid.Empty);

        var result = await _appService.ContratarPropostaAsync(request);

        Assert.False(result.IsSuccess);
        Assert.Equal("ID da proposta é obrigatório", result.Error);
        Assert.Equal("INVALID_PROPOSTA_ID", result.ErrorCode);
        _mockCommandPublisher.Verify(x => x.PublishAsync(It.IsAny<ContratarPropostaCommand>()), Times.Never);
    }

    [Fact]
    public async Task ContratarPropostaAsync_ComExcecaoArgumentException_DeveRetornarErro()
    {
        var propostaId = Guid.NewGuid();
        var request = new ContratarPropostaRequest(propostaId);

        _mockCommandPublisher.Setup(x => x.PublishAsync(It.IsAny<ContratarPropostaCommand>()))
            .ThrowsAsync(new ArgumentException("Argumento inválido"));

        var result = await _appService.ContratarPropostaAsync(request);

        Assert.False(result.IsSuccess);
        Assert.Equal("Argumento inválido", result.Error);
        Assert.Equal("INVALID_ARGUMENT", result.ErrorCode);
    }

    [Fact]
    public async Task ContratarPropostaAsync_ComExcecaoInvalidOperationException_DeveRetornarErro()
    {
        var propostaId = Guid.NewGuid();
        var request = new ContratarPropostaRequest(propostaId);

        _mockCommandPublisher.Setup(x => x.PublishAsync(It.IsAny<ContratarPropostaCommand>()))
            .ThrowsAsync(new InvalidOperationException("Operação inválida"));

        var result = await _appService.ContratarPropostaAsync(request);

        Assert.False(result.IsSuccess);
        Assert.Equal("Operação inválida", result.Error);
        Assert.Equal("INVALID_OPERATION", result.ErrorCode);
    }

    [Fact]
    public async Task ObterContratacaoPorIdAsync_ComIdExistente_DeveRetornarContratacao()
    {
        var contratacaoId = Guid.NewGuid();
        var propostaId = Guid.NewGuid();
        var contratacao = new Contratacao(propostaId, "João Silva", 100000m, 500m, _mockValidationService.Object);

        _mockRepository.Setup(x => x.GetByIdAsync(contratacaoId))
            .ReturnsAsync(contratacao);

        var result = await _appService.ObterContratacaoPorIdAsync(contratacaoId);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(propostaId, result.Value.PropostaId);
        Assert.Equal("João Silva", result.Value.NomeCliente);
        Assert.Equal(100000m, result.Value.ValorCobertura);
        Assert.Equal(500m, result.Value.ValorPremio);
    }

    [Fact]
    public async Task ObterContratacaoPorIdAsync_ComIdInexistente_DeveRetornarErro()
    {
        var contratacaoId = Guid.NewGuid();

        _mockRepository.Setup(x => x.GetByIdAsync(contratacaoId))
            .ReturnsAsync((Contratacao?)null);

        var result = await _appService.ObterContratacaoPorIdAsync(contratacaoId);

        Assert.False(result.IsSuccess);
        Assert.Equal("Contratação não encontrada", result.Error);
        Assert.Equal("NOT_FOUND", result.ErrorCode);
        Assert.Null(result.Value);
    }

    [Fact]
    public async Task ObterContratacaoPorIdAsync_ComIdVazio_DeveRetornarErro()
    {
        var result = await _appService.ObterContratacaoPorIdAsync(Guid.Empty);

        Assert.False(result.IsSuccess);
        Assert.Equal("ID é obrigatório", result.Error);
        Assert.Equal("INVALID_ID", result.ErrorCode);
        Assert.Null(result.Value);
    }

    [Fact]
    public async Task ListarContratacoesAsync_ComContratacoes_DeveRetornarLista()
    {
        var request = new PagedRequest { PageNumber = 1, PageSize = 10 };
        var contratacoes = new List<Contratacao>
        {
            new Contratacao(Guid.NewGuid(), "Cliente 1", 100000m, 500m, _mockValidationService.Object),
            new Contratacao(Guid.NewGuid(), "Cliente 2", 200000m, 1000m, _mockValidationService.Object),
            new Contratacao(Guid.NewGuid(), "Cliente 3", 150000m, 750m, _mockValidationService.Object)
        };

        var pagedResult = new PagedResult<Contratacao>
        {
            Items = contratacoes,
            TotalCount = 3,
            PageNumber = 1,
            PageSize = 10
        };

        _mockRepository.Setup(x => x.GetAllAsync(request))
            .ReturnsAsync(pagedResult);

        var result = await _appService.ListarContratacoesAsync(request);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(3, result.Value.Items.Count());
        Assert.Equal(3, result.Value.TotalCount);
        Assert.Equal(1, result.Value.PageNumber);
        Assert.Equal(10, result.Value.PageSize);
    }

    [Fact]
    public async Task ListarContratacoesAsync_SemContratacoes_DeveRetornarListaVazia()
    {
        var request = new PagedRequest { PageNumber = 1, PageSize = 10 };
        var pagedResult = new PagedResult<Contratacao>
        {
            Items = new List<Contratacao>(),
            TotalCount = 0,
            PageNumber = 1,
            PageSize = 10
        };

        _mockRepository.Setup(x => x.GetAllAsync(request))
            .ReturnsAsync(pagedResult);

        var result = await _appService.ListarContratacoesAsync(request);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Empty(result.Value.Items);
        Assert.Equal(0, result.Value.TotalCount);
    }

    [Fact]
    public void Constructor_ComRepositoryNull_DeveLancarExcecao()
    {
        Assert.Throws<ArgumentNullException>(() => new ContratacaoAppService(null!, _mockCommandPublisher.Object));
    }

    [Fact]
    public void Constructor_ComCommandPublisherNull_DeveLancarExcecao()
    {
        Assert.Throws<ArgumentNullException>(() => new ContratacaoAppService(_mockRepository.Object, null!));
    }

    [Fact]
    public async Task ContratarPropostaAsync_DeveUsarCommandPublisherCorretamente()
    {
        var propostaId = Guid.NewGuid();
        var request = new ContratarPropostaRequest(propostaId);

        await _appService.ContratarPropostaAsync(request);

        _mockCommandPublisher.Verify(x => x.PublishAsync(It.Is<ContratarPropostaCommand>(
            cmd => cmd.PropostaId == propostaId)), Times.Once);
    }

    [Fact]
    public async Task ObterContratacaoPorIdAsync_DeveUsarRepositoryCorretamente()
    {
        var contratacaoId = Guid.NewGuid();
        var propostaId = Guid.NewGuid();
        var contratacao = new Contratacao(propostaId, "Teste Cliente", 50000m, 250m, _mockValidationService.Object);

        _mockRepository.Setup(x => x.GetByIdAsync(contratacaoId))
            .ReturnsAsync(contratacao);

        var result = await _appService.ObterContratacaoPorIdAsync(contratacaoId);

        Assert.True(result.IsSuccess);
        _mockRepository.Verify(x => x.GetByIdAsync(contratacaoId), Times.Once);
    }

    [Fact]
    public async Task ListarContratacoesAsync_DeveUsarRepositoryCorretamente()
    {
        var request = new PagedRequest { PageNumber = 2, PageSize = 5 };
        var pagedResult = new PagedResult<Contratacao>
        {
            Items = new List<Contratacao>(),
            TotalCount = 0,
            PageNumber = 2,
            PageSize = 5
        };

        _mockRepository.Setup(x => x.GetAllAsync(request))
            .ReturnsAsync(pagedResult);

        var result = await _appService.ListarContratacoesAsync(request);

        Assert.True(result.IsSuccess);
        _mockRepository.Verify(x => x.GetAllAsync(request), Times.Once);
    }
}
