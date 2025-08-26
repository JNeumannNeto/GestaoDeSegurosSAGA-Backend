using ContratacaoService.Domain.Entities;
using ContratacaoService.Domain.Enums;
using ContratacaoService.Domain.Models;
using ContratacaoService.Domain.Ports;
using Shared.Messaging.Abstractions;
using Shared.Messaging.Commands;
using Shared.Messaging.Events;

namespace ContratacaoService.Application.Handlers;

public class ContratarPropostaCommandHandler : ICommandHandler<ContratarPropostaCommand>
{
    private readonly IContratacaoRepository _contratacaoRepository;
    private readonly IPropostaServiceClient _propostaServiceClient;
    private readonly IEventPublisher _eventPublisher;
    private readonly IValidationService _validationService;

    public ContratarPropostaCommandHandler(
        IContratacaoRepository contratacaoRepository,
        IPropostaServiceClient propostaServiceClient,
        IEventPublisher eventPublisher,
        IValidationService validationService)
    {
        _contratacaoRepository = contratacaoRepository ?? throw new ArgumentNullException(nameof(contratacaoRepository));
        _propostaServiceClient = propostaServiceClient ?? throw new ArgumentNullException(nameof(propostaServiceClient));
        _eventPublisher = eventPublisher ?? throw new ArgumentNullException(nameof(eventPublisher));
        _validationService = validationService ?? throw new ArgumentNullException(nameof(validationService));
    }

    public async Task HandleAsync(ContratarPropostaCommand command)
    {
        try
        {
            var resultado = await ProcessarContratacao(command.PropostaId);
            await PublicarEvento(resultado);
        }
        catch (Exception ex)
        {
            var eventoErro = CriarEventoErro(command.PropostaId, $"Erro interno: {ex.Message}");
            await _eventPublisher.PublishAsync(eventoErro);
        }
    }

    private async Task<ResultadoContratacao> ProcessarContratacao(Guid propostaId)
    {
        return await PropostaJaContratada(propostaId) switch
        {
            true => ResultadoContratacao.Erro(propostaId, "Proposta já foi contratada"),
            false => await ProcessarPropostaNova(propostaId)
        };
    }

    private async Task<ResultadoContratacao> ProcessarPropostaNova(Guid propostaId)
    {
        var proposta = await ObterProposta(propostaId);
        
        return proposta switch
        {
            null => ResultadoContratacao.Erro(propostaId, "Proposta não encontrada"),
            _ when !PropostaEstaAprovada(proposta) => ResultadoContratacao.ErroComProposta(proposta, "Apenas propostas aprovadas podem ser contratadas"),
            _ => await CriarContratacaoComSucesso(proposta)
        };
    }

    private async Task<ResultadoContratacao> CriarContratacaoComSucesso(PropostaDto proposta)
    {
        var contratacao = await CriarContratacao(proposta);
        return ResultadoContratacao.Sucesso(contratacao);
    }

    private async Task<bool> PropostaJaContratada(Guid propostaId)
    {
        var propostaExistente = await _contratacaoRepository.GetByPropostaIdAsync(propostaId);
        return propostaExistente != null;
    }

    private async Task<PropostaDto?> ObterProposta(Guid propostaId)
    {
        return await _propostaServiceClient.ObterPropostaPorIdAsync(propostaId);
    }

    private static bool PropostaEstaAprovada(PropostaDto proposta)
    {
        return proposta.Status == (int)StatusProposta.Aprovada;
    }

    private async Task<Contratacao> CriarContratacao(PropostaDto proposta)
    {
        var contratacao = new Contratacao(
            proposta.Id,
            proposta.NomeCliente,
            proposta.ValorCobertura,
            proposta.ValorPremio,
            _validationService
        );

        await _contratacaoRepository.AddAsync(contratacao);
        return contratacao;
    }

    private async Task PublicarEvento(ResultadoContratacao resultado)
    {
        var evento = resultado.TipoResultado switch
        {
            TipoResultadoContratacao.Sucesso => CriarEventoSucesso(resultado.Contratacao!),
            TipoResultadoContratacao.ErroSimples => CriarEventoErro(resultado.PropostaId, resultado.MensagemErro!),
            TipoResultadoContratacao.ErroComDados => CriarEventoErroComDados(resultado.Proposta!, resultado.MensagemErro!),
            _ => throw new InvalidOperationException("Tipo de resultado não suportado")
        };

        await _eventPublisher.PublishAsync(evento);
    }

    private static ContratacaoProcessadaEvent CriarEventoErro(Guid propostaId, string mensagem)
    {
        return new ContratacaoProcessadaEvent(
            Guid.Empty,
            propostaId,
            string.Empty,
            0,
            0,
            DateTime.UtcNow,
            false,
            mensagem
        );
    }

    private static ContratacaoProcessadaEvent CriarEventoErroComDados(PropostaDto proposta, string mensagem)
    {
        return new ContratacaoProcessadaEvent(
            Guid.Empty,
            proposta.Id,
            proposta.NomeCliente,
            proposta.ValorCobertura,
            proposta.ValorPremio,
            DateTime.UtcNow,
            false,
            mensagem
        );
    }

    private static ContratacaoProcessadaEvent CriarEventoSucesso(Contratacao contratacao)
    {
        return new ContratacaoProcessadaEvent(
            contratacao.Id,
            contratacao.PropostaId,
            contratacao.NomeCliente,
            contratacao.ValorCobertura,
            contratacao.ValorPremio,
            contratacao.DataContratacao,
            true
        );
    }
}

public enum TipoResultadoContratacao
{
    Sucesso,
    ErroSimples,
    ErroComDados
}

public class ResultadoContratacao
{
    public TipoResultadoContratacao TipoResultado { get; private set; }
    public Guid PropostaId { get; private set; }
    public string? MensagemErro { get; private set; }
    public PropostaDto? Proposta { get; private set; }
    public Contratacao? Contratacao { get; private set; }

    private ResultadoContratacao(TipoResultadoContratacao tipo, Guid propostaId, string? mensagem = null, PropostaDto? proposta = null, Contratacao? contratacao = null)
    {
        TipoResultado = tipo;
        PropostaId = propostaId;
        MensagemErro = mensagem;
        Proposta = proposta;
        Contratacao = contratacao;
    }

    public static ResultadoContratacao Sucesso(Contratacao contratacao)
    {
        return new ResultadoContratacao(TipoResultadoContratacao.Sucesso, contratacao.PropostaId, contratacao: contratacao);
    }

    public static ResultadoContratacao Erro(Guid propostaId, string mensagem)
    {
        return new ResultadoContratacao(TipoResultadoContratacao.ErroSimples, propostaId, mensagem);
    }

    public static ResultadoContratacao ErroComProposta(PropostaDto proposta, string mensagem)
    {
        return new ResultadoContratacao(TipoResultadoContratacao.ErroComDados, proposta.Id, mensagem, proposta);
    }
}
