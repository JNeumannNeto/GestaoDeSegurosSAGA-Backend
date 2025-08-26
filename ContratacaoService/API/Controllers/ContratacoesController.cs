using ContratacaoService.API.DTOs;
using ContratacaoService.Application.DTOs;
using ContratacaoService.Application.Services;
using ContratacaoService.Domain.Common;
using Microsoft.AspNetCore.Mvc;

namespace ContratacaoService.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ContratacoesController : ControllerBase
{
    private readonly ContratacaoAppService _contratacaoService;

    public ContratacoesController(ContratacaoAppService contratacaoService)
    {
        _contratacaoService = contratacaoService ?? throw new ArgumentNullException(nameof(contratacaoService));
    }

    [HttpPost]
    public async Task<ActionResult<string>> ContratarProposta([FromBody] ContratarPropostaRequest request)
    {
        var result = await _contratacaoService.ContratarPropostaAsync(request);
        
        if (!result.IsSuccess)
        {
            var errorResponse = new ErrorResponse
            {
                Message = result.Error!,
                ErrorCode = result.ErrorCode
            };
            return BadRequest(errorResponse);
        }

        return Accepted(result.Value);
    }

    [HttpGet]
    public async Task<ActionResult<PagedResult<ContratacaoResponse>>> ListarContratacoes([FromQuery] PagedRequest request)
    {
        var result = await _contratacaoService.ListarContratacoesAsync(request);
        
        if (!result.IsSuccess)
        {
            var errorResponse = new ErrorResponse
            {
                Message = result.Error!,
                ErrorCode = result.ErrorCode
            };
            return BadRequest(errorResponse);
        }

        return Ok(result.Value);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ContratacaoResponse>> ObterContratacao(Guid id)
    {
        var result = await _contratacaoService.ObterContratacaoPorIdAsync(id);
        
        if (!result.IsSuccess)
        {
            var errorResponse = new ErrorResponse
            {
                Message = result.Error!,
                ErrorCode = result.ErrorCode
            };

            return result.ErrorCode == "NOT_FOUND" ? NotFound(errorResponse) : BadRequest(errorResponse);
        }

        return Ok(result.Value);
    }
}
