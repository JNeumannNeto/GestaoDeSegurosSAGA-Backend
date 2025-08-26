using PropostaService.Domain.Entities;
using PropostaService.Domain.Enums;
using PropostaService.Domain.Ports;

namespace PropostaService.Infrastructure.Data;

public class PropostaDataSeeder : IDataSeeder
{
    private readonly IPropostaRepository _repository;

    public PropostaDataSeeder(IPropostaRepository repository)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
    }

    public async Task SeedAsync()
    {
        var existingPropostas = await _repository.GetAllAsync();
        Console.WriteLine($"[SEEDER] Propostas existentes: {existingPropostas.Count()}");
        
        // Temporariamente removendo a verificação para forçar o seed
        // if (existingPropostas.Any())
        // {
        //     Console.WriteLine("[SEEDER] Já existem propostas, pulando seed");
        //     return;
        // }

        Console.WriteLine("[SEEDER] Iniciando seed de propostas...");
        await SeedPropostasIniciais();
        
        var propostasAposSeed = await _repository.GetAllAsync();
        Console.WriteLine($"[SEEDER] Propostas após seed: {propostasAposSeed.Count()}");
    }

    private async Task SeedPropostasIniciais()
    {
        try
        {
            Console.WriteLine("[SEEDER] Criando propostas...");
            
            var proposta1 = CriarPropostaAprovada();
            Console.WriteLine($"[SEEDER] Proposta 1 criada: {proposta1.Id} - {proposta1.NomeCliente.Valor}");
            
            var proposta2 = CriarPropostaEmAnalise();
            Console.WriteLine($"[SEEDER] Proposta 2 criada: {proposta2.Id} - {proposta2.NomeCliente.Valor}");
            
            var proposta3 = CriarPropostaRejeitada();
            Console.WriteLine($"[SEEDER] Proposta 3 criada: {proposta3.Id} - {proposta3.NomeCliente.Valor}");

            var propostas = new[] { proposta1, proposta2, proposta3 };

            foreach (var proposta in propostas)
            {
                Console.WriteLine($"[SEEDER] Adicionando proposta: {proposta.Id}");
                await _repository.AddAsync(proposta);
                Console.WriteLine($"[SEEDER] Proposta adicionada com sucesso: {proposta.Id}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[SEEDER] ERRO ao criar propostas: {ex.Message}");
            Console.WriteLine($"[SEEDER] Stack trace: {ex.StackTrace}");
        }
    }

    private static Proposta CriarPropostaAprovada()
    {
        var proposta = new Proposta("João Silva", TipoCliente.PessoaFisica, TipoSeguro.Vida, 100000m, 500m);
        proposta.AlterarStatus(StatusProposta.Aprovada);
        return proposta;
    }

    private static Proposta CriarPropostaEmAnalise()
    {
        return new Proposta("Maria Santos", TipoCliente.PessoaFisica, TipoSeguro.Automovel, 50000m, 800m);
    }

    private static Proposta CriarPropostaRejeitada()
    {
        var proposta = new Proposta("Empresa ABC Ltda", TipoCliente.PessoaJuridica, TipoSeguro.Empresarial, 500000m, 2000m);
        proposta.AlterarStatus(StatusProposta.Rejeitada);
        return proposta;
    }
}
