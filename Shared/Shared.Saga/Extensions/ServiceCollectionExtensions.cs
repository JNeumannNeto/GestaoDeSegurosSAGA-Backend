using Microsoft.Extensions.DependencyInjection;
using Shared.Saga.Abstractions;
using Shared.Saga.Orchestration;
using Shared.Saga.Storage;

namespace Shared.Saga.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddSaga(this IServiceCollection services)
    {
        services.AddSingleton<ISagaRepository, InMemorySagaRepository>();
        services.AddScoped<ISagaOrchestrator, SagaOrchestrator>();
        
        return services;
    }

    public static IServiceCollection AddSaga<TSagaRepository>(this IServiceCollection services)
        where TSagaRepository : class, ISagaRepository
    {
        services.AddScoped<ISagaRepository, TSagaRepository>();
        services.AddScoped<ISagaOrchestrator, SagaOrchestrator>();
        
        return services;
    }
}
