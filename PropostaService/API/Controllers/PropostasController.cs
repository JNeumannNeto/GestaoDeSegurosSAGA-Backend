using Microsoft.AspNetCore.Mvc;
using PropostaService.Application.Commands;
using PropostaService.Application.DTOs;
using PropostaService.Application.Queries;
using PropostaService.Application.Services;

namespace PropostaService.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PropostasController : ControllerBase
{
    private readonly IMediator _mediator;

    public PropostasController(IMediator mediator)
    {
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
    }

    [HttpPost]
    public async Task<ActionResult<PropostaResponse>> CriarProposta([FromBody] CriarPropostaRequest request)
    {
        try
        {
            var command = new CriarPropostaCommand(
                request.NomeCliente,
                request.TipoCliente,
                request.TipoSeguro,
                request.ValorCobertura,
                request.ValorPremio
            );

            var proposta = await _mediator.SendAsync(command);
            return CreatedAtAction(nameof(ObterProposta), new { id = proposta.Id }, proposta);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<PropostaResponse>> ObterProposta(Guid id)
    {
        var query = new ObterPropostaPorIdQuery(id);
        var proposta = await _mediator.SendAsync(query);
        
        if (proposta == null)
            return NotFound();

        return Ok(proposta);
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<PropostaResponse>>> ListarPropostas()
    {
        Console.WriteLine("[CONTROLLER] Recebida requisição para listar propostas");
        var query = new ListarPropostasQuery();
        Console.WriteLine("[CONTROLLER] Query criada, enviando para mediator...");
        var propostas = await _mediator.SendAsync(query);
        Console.WriteLine($"[CONTROLLER] Propostas recebidas do mediator: {propostas?.Count() ?? 0}");
        return Ok(propostas);
    }

    [HttpPut("{id}/status")]
    public async Task<ActionResult<PropostaResponse>> AlterarStatus(Guid id, [FromBody] AlterarStatusRequest request)
    {
        var command = new AlterarStatusPropostaCommand(id, request.NovoStatus);
        var proposta = await _mediator.SendAsync(command);
        
        if (proposta == null)
            return NotFound();

        return Ok(proposta);
    }
}
