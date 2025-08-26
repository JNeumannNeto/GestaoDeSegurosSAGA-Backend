using ContratacaoService.Domain.Common;
using ContratacaoService.Domain.Models;
using ContratacaoService.Domain.Ports;
using System.Text.Json;

namespace ContratacaoService.Infrastructure.Clients;

public class PropostaServiceClient : IPropostaServiceClient
{
    private readonly HttpClient _httpClient;
    private readonly JsonSerializerOptions _jsonOptions;

    public PropostaServiceClient(HttpClient httpClient)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
    }

    public async Task<PropostaDto?> ObterPropostaPorIdAsync(Guid propostaId)
    {
        var result = await ObterPropostaPorIdWithResultAsync(propostaId);
        return result.IsSuccess ? result.Value : null;
    }

    public async Task<Result<PropostaDto>> ObterPropostaPorIdWithResultAsync(Guid propostaId)
    {
        if (propostaId == Guid.Empty)
            return Result<PropostaDto>.Failure("ID da proposta é obrigatório", "INVALID_PROPOSTA_ID");

        try
        {
            var response = await _httpClient.GetAsync($"api/propostas/{propostaId}");
            
            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                return Result<PropostaDto>.Failure("Proposta não encontrada", "PROPOSTA_NOT_FOUND");

            if (!response.IsSuccessStatusCode)
                return Result<PropostaDto>.Failure($"Erro na comunicação com o serviço de propostas: {response.StatusCode}", "SERVICE_ERROR");

            var json = await response.Content.ReadAsStringAsync();
            var proposta = JsonSerializer.Deserialize<PropostaDto>(json, _jsonOptions);
            
            if (proposta == null)
                return Result<PropostaDto>.Failure("Erro ao deserializar resposta do serviço", "DESERIALIZATION_ERROR");

            return Result<PropostaDto>.Success(proposta);
        }
        catch (HttpRequestException ex)
        {
            return Result<PropostaDto>.Failure($"Erro de conexão com o serviço de propostas: {ex.Message}", "CONNECTION_ERROR");
        }
        catch (TaskCanceledException ex)
        {
            return Result<PropostaDto>.Failure($"Timeout na comunicação com o serviço de propostas: {ex.Message}", "TIMEOUT_ERROR");
        }
        catch (JsonException ex)
        {
            return Result<PropostaDto>.Failure($"Erro ao processar resposta do serviço: {ex.Message}", "JSON_ERROR");
        }
    }
}
