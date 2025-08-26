using ContratacaoService.Application.Handlers;
using ContratacaoService.Domain.Entities;
using ContratacaoService.Domain.Enums;
using ContratacaoService.Domain.Models;
using ContratacaoService.Domain.Ports;
using Moq;
using Shared.Messaging.Abstractions;
using Shared.Messaging.Commands;
using Shared.Messaging.Events;
using Xunit;

namespace ContratacaoService.Tests.Application;

public class ContratarPropostaCommandHandlerTests
{
    private readonly Mock<IContratacaoRepository> _mockRepository;
    private readonly Mock<IPropostaServiceClient> _mockPropostaClient;
    private readonly Mock<IEventPublisher> _mockEventPublisher;
    private readonly Mock<IValidationService> _mockValidationService;
    private readonly ContratarPropostaCommandHandler _handler;

    public ContratarPropostaCommandHandlerTests()
    {
        _mockRepository = new Mock<IContratacaoRepository>();
        _mockPropostaClient = new Mock<IPropostaServiceClient>();
        _mockEventPublisher = new Mock<IEventPublisher>();
        _mockValidationService = new Mock<IValidationService>();
        _handler = new ContratarPropostaCommandHandler(
            _mockRepository.Object, 
            _mockPropostaClient.Object, 
            _mockEventPublisher.Object, 
            _mockValidationService.Object);
    }

    [Fact]
    public async Task HandleAsync_ComPropostaAprovada_DeveCriarContratacao()
    {
        var propostaId = Guid.NewGuid();
        var command = new ContratarPropostaCommand(propostaId);
        var propostaDto = new PropostaDto(
            propostaId,
            "João Silva",
            1,
            1,
            100000m,
            500m,
            (int)StatusProposta.Aprovada,
            DateTime.UtcNow
        );

        _mockRepository.Setup(x => x.GetByPropostaIdAsync(propostaId))
            .ReturnsAsync((Contratacao?)null);

        _mockPropostaClient.Setup(x => x.ObterPropostaPorIdAsync(propostaId))
            .ReturnsAsync(propostaDto);

        await _handler.HandleAsync(command);

        _mockRepository.Verify(x => x.AddAsync(It.IsAny<Contratacao>()), Times.Once);
        _mockEventPublisher.Verify(x => x.PublishAsync(It.IsAny<ContratacaoProcessadaEvent>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_ComPropostaNaoEncontrada_DevePublicarEventoErro()
    {
        var propostaId = Guid.NewGuid();
        var command = new ContratarPropostaCommand(propostaId);

        _mockRepository.Setup(x => x.GetByPropostaIdAsync(propostaId))
            .ReturnsAsync((Contratacao?)null);

        _mockPropostaClient.Setup(x => x.ObterPropostaPorIdAsync(propostaId))
            .ReturnsAsync((PropostaDto?)null);

        await _handler.HandleAsync(command);

        _mockRepository.Verify(x => x.AddAsync(It.IsAny<Contratacao>()), Times.Never);
        _mockEventPublisher.Verify(x => x.PublishAsync(It.Is<ContratacaoProcessadaEvent>(e => 
            e.PropostaId == propostaId && 
            !e.Sucesso && 
            e.MensagemErro == "Proposta não encontrada")), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_ComPropostaEmAnalise_DevePublicarEventoErro()
    {
        var propostaId = Guid.NewGuid();
        var command = new ContratarPropostaCommand(propostaId);
        var propostaDto = new PropostaDto(
            propostaId,
            "Maria Santos",
            1,
            1,
            50000m,
            300m,
            (int)StatusProposta.EmAnalise,
            DateTime.UtcNow
        );

        _mockRepository.Setup(x => x.GetByPropostaIdAsync(propostaId))
            .ReturnsAsync((Contratacao?)null);

        _mockPropostaClient.Setup(x => x.ObterPropostaPorIdAsync(propostaId))
            .ReturnsAsync(propostaDto);

        await _handler.HandleAsync(command);

        _mockRepository.Verify(x => x.AddAsync(It.IsAny<Contratacao>()), Times.Never);
        _mockEventPublisher.Verify(x => x.PublishAsync(It.Is<ContratacaoProcessadaEvent>(e => 
            e.PropostaId == propostaId && 
            !e.Sucesso && 
            e.MensagemErro == "Apenas propostas aprovadas podem ser contratadas")), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_ComPropostaRejeitada_DevePublicarEventoErro()
    {
        var propostaId = Guid.NewGuid();
        var command = new ContratarPropostaCommand(propostaId);
        var propostaDto = new PropostaDto(
            propostaId,
            "Pedro Costa",
            1,
            1,
            75000m,
            400m,
            (int)StatusProposta.Rejeitada,
            DateTime.UtcNow
        );

        _mockRepository.Setup(x => x.GetByPropostaIdAsync(propostaId))
            .ReturnsAsync((Contratacao?)null);

        _mockPropostaClient.Setup(x => x.ObterPropostaPorIdAsync(propostaId))
            .ReturnsAsync(propostaDto);

        await _handler.HandleAsync(command);

        _mockRepository.Verify(x => x.AddAsync(It.IsAny<Contratacao>()), Times.Never);
        _mockEventPublisher.Verify(x => x.PublishAsync(It.Is<ContratacaoProcessadaEvent>(e => 
            e.PropostaId == propostaId && 
            !e.Sucesso && 
            e.MensagemErro == "Apenas propostas aprovadas podem ser contratadas")), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_ComPropostaJaContratada_DevePublicarEventoErro()
    {
        var propostaId = Guid.NewGuid();
        var command = new ContratarPropostaCommand(propostaId);
        var contratacaoExistente = new Contratacao(propostaId, "Cliente Teste", 100000m, 500m, _mockValidationService.Object);

        _mockRepository.Setup(x => x.GetByPropostaIdAsync(propostaId))
            .ReturnsAsync(contratacaoExistente);

        await _handler.HandleAsync(command);

        _mockRepository.Verify(x => x.AddAsync(It.IsAny<Contratacao>()), Times.Never);
        _mockEventPublisher.Verify(x => x.PublishAsync(It.Is<ContratacaoProcessadaEvent>(e => 
            e.PropostaId == propostaId && 
            !e.Sucesso && 
            e.MensagemErro == "Proposta já foi contratada")), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_ComExcecaoNoRepositorio_DevePublicarEventoErro()
    {
        var propostaId = Guid.NewGuid();
        var command = new ContratarPropostaCommand(propostaId);
        var propostaDto = new PropostaDto(
            propostaId,
            "Ana Silva",
            1,
            1,
            120000m,
            600m,
            (int)StatusProposta.Aprovada,
            DateTime.UtcNow
        );

        _mockRepository.Setup(x => x.GetByPropostaIdAsync(propostaId))
            .ReturnsAsync((Contratacao?)null);

        _mockPropostaClient.Setup(x => x.ObterPropostaPorIdAsync(propostaId))
            .ReturnsAsync(propostaDto);

        _mockRepository.Setup(x => x.AddAsync(It.IsAny<Contratacao>()))
            .ThrowsAsync(new InvalidOperationException("Erro no banco de dados"));

        await _handler.HandleAsync(command);

        _mockEventPublisher.Verify(x => x.PublishAsync(It.Is<ContratacaoProcessadaEvent>(e => 
            e.PropostaId == propostaId && 
            !e.Sucesso && 
            e.MensagemErro.Contains("Erro interno: Erro no banco de dados"))), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_ComExcecaoNoClienteProposta_DevePublicarEventoErro()
    {
        var propostaId = Guid.NewGuid();
        var command = new ContratarPropostaCommand(propostaId);

        _mockRepository.Setup(x => x.GetByPropostaIdAsync(propostaId))
            .ReturnsAsync((Contratacao?)null);

        _mockPropostaClient.Setup(x => x.ObterPropostaPorIdAsync(propostaId))
            .ThrowsAsync(new HttpRequestException("Serviço indisponível"));

        await _handler.HandleAsync(command);

        _mockRepository.Verify(x => x.AddAsync(It.IsAny<Contratacao>()), Times.Never);
        _mockEventPublisher.Verify(x => x.PublishAsync(It.Is<ContratacaoProcessadaEvent>(e => 
            e.PropostaId == propostaId && 
            !e.Sucesso && 
            e.MensagemErro.Contains("Erro interno: Serviço indisponível"))), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_ComSucesso_DevePublicarEventoSucesso()
    {
        var propostaId = Guid.NewGuid();
        var command = new ContratarPropostaCommand(propostaId);
        var propostaDto = new PropostaDto(
            propostaId,
            "Cliente Sucesso",
            1,
            1,
            200000m,
            1000m,
            (int)StatusProposta.Aprovada,
            DateTime.UtcNow
        );

        _mockRepository.Setup(x => x.GetByPropostaIdAsync(propostaId))
            .ReturnsAsync((Contratacao?)null);

        _mockPropostaClient.Setup(x => x.ObterPropostaPorIdAsync(propostaId))
            .ReturnsAsync(propostaDto);

        await _handler.HandleAsync(command);

        _mockRepository.Verify(x => x.AddAsync(It.IsAny<Contratacao>()), Times.Once);
        _mockEventPublisher.Verify(x => x.PublishAsync(It.Is<ContratacaoProcessadaEvent>(e => 
            e.PropostaId == propostaId && 
            e.Sucesso && 
            e.NomeCliente == "Cliente Sucesso" &&
            e.ValorCobertura == 200000m &&
            e.ValorPremio == 1000m)), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_DeveVerificarSePropostaJaFoiContratada()
    {
        var propostaId = Guid.NewGuid();
        var command = new ContratarPropostaCommand(propostaId);

        await _handler.HandleAsync(command);

        _mockRepository.Verify(x => x.GetByPropostaIdAsync(propostaId), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_ComPropostaAprovada_DeveUsarValidationService()
    {
        var propostaId = Guid.NewGuid();
        var command = new ContratarPropostaCommand(propostaId);
        var propostaDto = new PropostaDto(
            propostaId,
            "Cliente Validação",
            1,
            1,
            150000m,
            750m,
            (int)StatusProposta.Aprovada,
            DateTime.UtcNow
        );

        _mockRepository.Setup(x => x.GetByPropostaIdAsync(propostaId))
            .ReturnsAsync((Contratacao?)null);

        _mockPropostaClient.Setup(x => x.ObterPropostaPorIdAsync(propostaId))
            .ReturnsAsync(propostaDto);

        await _handler.HandleAsync(command);

        _mockRepository.Verify(x => x.AddAsync(It.Is<Contratacao>(c => 
            c.PropostaId == propostaId &&
            c.NomeCliente == "Cliente Validação" &&
            c.ValorCobertura == 150000m &&
            c.ValorPremio == 750m)), Times.Once);
    }
}
