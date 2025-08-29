using ContratacaoService.Application.Sagas.ContratarPropostaSaga.Models;
using ContratacaoService.Domain.Entities;
using ContratacaoService.Domain.Ports;
using Microsoft.Extensions.Logging;
using Shared.Saga.Abstractions;
using Shared.Saga.Models;

namespace ContratacaoService.Application.Sagas.ContratarPropostaSaga.Steps;

public class CriarContratacaoStep : ISagaStep<ContratarPropostaData>
{
    private readonly IContratacaoRepository _contratacaoRepository;
    private readonly IValidationService _validationService;
    private readonly ILogger<CriarContratacaoStep> _logger;

    public string StepName => "CriarContratacao";
    public int Order => 3;

    public CriarContratacaoStep(
        IContratacaoRepository contratacaoRepository,
        IValidationService validationService,
        ILogger<CriarContratacaoStep> logger)
    {
        _contratacaoRepository = contratacaoRepository ?? throw new ArgumentNullException(nameof(contratacaoRepository));
        _validationService = validationService ?? throw new ArgumentNullException(nameof(validationService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<StepResult> ExecuteAsync(ContratarPropostaData data, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Criando contratação para proposta {PropostaId}", data.PropostaId);

        try
        {
            if (data.Proposta == null)
            {
                var errorMessage = "Proposta não foi carregada nas etapas anteriores";
                _logger.LogError(errorMessage);
                data.ErrorMessage = errorMessage;
                return StepResult.Failed(errorMessage);
            }

            var contratacao = new Contratacao(
                data.Proposta.Id,
                data.Proposta.NomeCliente,
                data.Proposta.ValorCobertura,
                data.Proposta.ValorPremio,
                _validationService
            );

            await _contratacaoRepository.AddAsync(contratacao);
            data.Contratacao = contratacao;

            _logger.LogInformation("Contratação {ContratacaoId} criada com sucesso para proposta {PropostaId}", 
                contratacao.Id, data.PropostaId);
            
            return StepResult.Success();
        }
        catch (Exception ex)
        {
            var errorMessage = $"Erro ao criar contratação: {ex.Message}";
            _logger.LogError(ex, "Erro ao criar contratação para proposta {PropostaId}", data.PropostaId);
            data.ErrorMessage = errorMessage;
            return StepResult.Failed(errorMessage, ex);
        }
    }

    public async Task<StepResult> CompensateAsync(ContratarPropostaData data, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Compensando criação de contratação para proposta {PropostaId}", data.PropostaId);

        try
        {
            if (data.Contratacao != null)
            {
                await _contratacaoRepository.DeleteAsync(data.Contratacao.Id);
                _logger.LogInformation("Contratação {ContratacaoId} removida com sucesso", data.Contratacao.Id);
                data.Contratacao = null;
            }

            return StepResult.Success();
        }
        catch (Exception ex)
        {
            var errorMessage = $"Erro ao compensar criação de contratação: {ex.Message}";
            _logger.LogError(ex, "Erro ao compensar criação de contratação para proposta {PropostaId}", data.PropostaId);
            return StepResult.Failed(errorMessage, ex);
        }
    }
}
