using ContratacaoService.Application.DTOs;
using ContratacaoService.Domain.Common;
using ContratacaoService.Domain.Entities;
using ContratacaoService.Domain.Ports;
using Shared.Messaging.Abstractions;
using Shared.Messaging.Commands;

namespace ContratacaoService.Application.Services;

public class ContratacaoAppService
{
    private readonly IContratacaoRepository _contratacaoRepository;
    private readonly ICommandPublisher _commandPublisher;

    public ContratacaoAppService(
        IContratacaoRepository contratacaoRepository,
        ICommandPublisher commandPublisher)
    {
        _contratacaoRepository = contratacaoRepository ?? throw new ArgumentNullException(nameof(contratacaoRepository));
        _commandPublisher = commandPublisher ?? throw new ArgumentNullException(nameof(commandPublisher));
    }

    public async Task<Result<string>> ContratarPropostaAsync(ContratarPropostaRequest request)
    {
        if (request.PropostaId == Guid.Empty)
            return Result<string>.Failure("ID da proposta é obrigatório", "INVALID_PROPOSTA_ID");

        try
        {
            var command = new ContratarPropostaCommand(request.PropostaId);
            await _commandPublisher.PublishAsync(command);
            
            return Result<string>.Success($"Solicitação de contratação da proposta {request.PropostaId} foi enviada para processamento assíncrono.");
        }
        catch (ArgumentException ex)
        {
            return Result<string>.Failure(ex.Message, "INVALID_ARGUMENT");
        }
        catch (InvalidOperationException ex)
        {
            return Result<string>.Failure(ex.Message, "INVALID_OPERATION");
        }
    }

    public async Task<Result<PagedResult<ContratacaoResponse>>> ListarContratacoesAsync(PagedRequest request)
    {
        var contratacoes = await _contratacaoRepository.GetAllAsync(request);
        
        var response = new PagedResult<ContratacaoResponse>
        {
            Items = contratacoes.Items.Select(MapToResponse),
            TotalCount = contratacoes.TotalCount,
            PageNumber = contratacoes.PageNumber,
            PageSize = contratacoes.PageSize
        };

        return Result<PagedResult<ContratacaoResponse>>.Success(response);
    }

    public async Task<Result<ContratacaoResponse>> ObterContratacaoPorIdAsync(Guid id)
    {
        if (id == Guid.Empty)
            return Result<ContratacaoResponse>.Failure("ID é obrigatório", "INVALID_ID");

        var contratacao = await _contratacaoRepository.GetByIdAsync(id);
        
        if (contratacao == null)
            return Result<ContratacaoResponse>.Failure("Contratação não encontrada", "NOT_FOUND");

        return Result<ContratacaoResponse>.Success(MapToResponse(contratacao));
    }

    private static ContratacaoResponse MapToResponse(Contratacao contratacao)
    {
        return new ContratacaoResponse(
            contratacao.Id,
            contratacao.PropostaId,
            contratacao.DataContratacao,
            contratacao.NomeCliente,
            contratacao.ValorCobertura,
            contratacao.ValorPremio
        );
    }
}
