using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Moq;
using PropostaService.Application.Ports;
using PropostaService.Application.Services;
using PropostaService.Domain.Ports;
using PropostaService.Infrastructure.Repositories;
using PropostaService.Infrastructure.Data;
using PropostaService.Infrastructure.Services;
using Shared.Messaging.Abstractions;

namespace PropostaService.Tests.Integration;

public class CustomWebApplicationFactory<TStartup> : WebApplicationFactory<TStartup> where TStartup : class
{
    private bool _dataSeeded = false;

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            services.RemoveAll<IEventPublisher>();
            
            var mockEventPublisher = new Mock<IEventPublisher>();
            services.AddSingleton(mockEventPublisher.Object);
            
            services.AddSingleton<IPropostaRepository, PropostaRepository>();
            services.AddScoped<IPropostaAppService, PropostaAppService>();
            services.AddScoped<IValidationService, ValidationService>();
            services.AddScoped<IDataSeeder, PropostaDataSeeder>();
            services.AddScoped<IMediator, PropostaService.Infrastructure.Services.Mediator>();

            services.AddScoped<PropostaService.Application.Handlers.ICommandHandler<PropostaService.Application.Commands.CriarPropostaCommand, PropostaService.Application.DTOs.PropostaResponse>, PropostaService.Application.Handlers.CriarPropostaCommandHandler>();
            services.AddScoped<PropostaService.Application.Handlers.IQueryHandler<PropostaService.Application.Queries.ObterPropostaPorIdQuery, PropostaService.Application.DTOs.PropostaResponse?>, PropostaService.Application.Handlers.ObterPropostaPorIdQueryHandler>();
            services.AddScoped<PropostaService.Application.Handlers.IQueryHandler<PropostaService.Application.Queries.ListarPropostasQuery, System.Collections.Generic.IEnumerable<PropostaService.Application.DTOs.PropostaResponse>>, PropostaService.Application.Handlers.ListarPropostasQueryHandler>();
            services.AddScoped<PropostaService.Application.Handlers.ICommandHandler<PropostaService.Application.Commands.AlterarStatusPropostaCommand, PropostaService.Application.DTOs.PropostaResponse?>, PropostaService.Application.Handlers.AlterarStatusPropostaCommandHandler>();
        });

        builder.UseEnvironment("Testing");
    }

    public new HttpClient CreateClient()
    {
        var client = base.CreateClient();
        
        if (!_dataSeeded)
        {
            using var scope = Services.CreateScope();
            var dataSeeder = scope.ServiceProvider.GetRequiredService<IDataSeeder>();
            dataSeeder.SeedAsync().GetAwaiter().GetResult();
            _dataSeeded = true;
        }
        
        return client;
    }
}
