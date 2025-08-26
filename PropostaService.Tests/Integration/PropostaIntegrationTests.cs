using System.Net;
using System.Net.Http.Json;
using Microsoft.Extensions.DependencyInjection;
using PropostaService.Application.DTOs;
using PropostaService.Domain.Enums;
using PropostaService.Domain.Ports;
using Xunit;

namespace PropostaService.Tests.Integration;

public class PropostaIntegrationTests : IClassFixture<CustomWebApplicationFactory<Program>>
{
    private readonly CustomWebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public PropostaIntegrationTests(CustomWebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _client = _factory.CreateClient();
    }

    [Fact]
    public async Task CriarProposta_ComDadosValidos_DeveRetornarCreated()
    {
        var request = new CriarPropostaRequest(
            "João Silva",
            TipoCliente.PessoaFisica,
            TipoSeguro.Vida,
            100000m,
            500m
        );

        var response = await _client.PostAsJsonAsync("/api/propostas", request);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        
        var proposta = await response.Content.ReadFromJsonAsync<PropostaResponse>();
        Assert.NotNull(proposta);
        Assert.Equal(request.NomeCliente, proposta.NomeCliente);
        Assert.Equal(request.TipoCliente, proposta.TipoCliente);
        Assert.Equal(request.TipoSeguro, proposta.TipoSeguro);
        Assert.Equal(request.ValorCobertura, proposta.ValorCobertura);
        Assert.Equal(request.ValorPremio, proposta.ValorPremio);
        Assert.Equal(StatusProposta.EmAnalise, proposta.Status);

        using var scope = _factory.Services.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IPropostaRepository>();
        var propostaRepository = await repository.GetByIdAsync(proposta.Id);
        
        Assert.NotNull(propostaRepository);
        Assert.Equal(proposta.NomeCliente, propostaRepository.NomeCliente.Valor);
        Assert.Equal(proposta.TipoCliente, propostaRepository.TipoCliente);
        Assert.Equal(proposta.TipoSeguro, propostaRepository.TipoSeguro);
        Assert.Equal(proposta.ValorCobertura, propostaRepository.ValorCobertura.Valor);
        Assert.Equal(proposta.ValorPremio, propostaRepository.ValorPremio.Valor);
        Assert.Equal(proposta.Status, propostaRepository.Status);
    }

    [Fact]
    public async Task ObterProposta_ComIdExistente_DeveRetornarProposta()
    {
        var createRequest = new CriarPropostaRequest(
            "Maria Santos",
            TipoCliente.PessoaFisica,
            TipoSeguro.Automovel,
            50000m,
            800m
        );

        var createResponse = await _client.PostAsJsonAsync("/api/propostas", createRequest);
        var createdProposta = await createResponse.Content.ReadFromJsonAsync<PropostaResponse>();

        var getResponse = await _client.GetAsync($"/api/propostas/{createdProposta!.Id}");

        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);
        
        var proposta = await getResponse.Content.ReadFromJsonAsync<PropostaResponse>();
        Assert.NotNull(proposta);
        Assert.Equal(createdProposta.Id, proposta.Id);
        Assert.Equal(createdProposta.NomeCliente, proposta.NomeCliente);
    }

    [Fact]
    public async Task ObterProposta_ComIdInexistente_DeveRetornarNotFound()
    {
        var response = await _client.GetAsync($"/api/propostas/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task ListarPropostas_DeveRetornarTodasPropostas()
    {
        var response = await _client.GetAsync("/api/propostas");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        var propostas = await response.Content.ReadFromJsonAsync<IEnumerable<PropostaResponse>>();
        Assert.NotNull(propostas);
        Assert.True(propostas.Count() >= 3);
    }

    [Fact]
    public async Task AlterarStatusProposta_ComDadosValidos_DeveAlterarStatus()
    {
        var createRequest = new CriarPropostaRequest(
            "Empresa XYZ",
            TipoCliente.PessoaJuridica,
            TipoSeguro.Empresarial,
            200000m,
            1500m
        );

        var createResponse = await _client.PostAsJsonAsync("/api/propostas", createRequest);
        var createdProposta = await createResponse.Content.ReadFromJsonAsync<PropostaResponse>();

        var alterarStatusRequest = new AlterarStatusRequest(StatusProposta.Aprovada);
        var alterarResponse = await _client.PutAsJsonAsync($"/api/propostas/{createdProposta!.Id}/status", alterarStatusRequest);

        Assert.Equal(HttpStatusCode.OK, alterarResponse.StatusCode);
        
        var propostaAlterada = await alterarResponse.Content.ReadFromJsonAsync<PropostaResponse>();
        Assert.NotNull(propostaAlterada);
        Assert.Equal(StatusProposta.Aprovada, propostaAlterada.Status);

        using var scope = _factory.Services.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IPropostaRepository>();
        var propostaRepository = await repository.GetByIdAsync(createdProposta.Id);
        
        Assert.NotNull(propostaRepository);
        Assert.Equal(StatusProposta.Aprovada, propostaRepository.Status);
    }

    [Fact]
    public async Task AlterarStatusProposta_ComIdInexistente_DeveRetornarNotFound()
    {
        var alterarStatusRequest = new AlterarStatusRequest(StatusProposta.Aprovada);
        var response = await _client.PutAsJsonAsync($"/api/propostas/{Guid.NewGuid()}/status", alterarStatusRequest);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task CriarProposta_ComDadosInvalidos_DeveRetornarBadRequest()
    {
        var request = new CriarPropostaRequest(
            "",
            TipoCliente.PessoaFisica,
            TipoSeguro.Vida,
            100000m,
            500m
        );

        var response = await _client.PostAsJsonAsync("/api/propostas", request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}
