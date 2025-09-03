using PropostaService.Application.Ports;
using PropostaService.Application.Services;
using PropostaService.Domain.Ports;
using PropostaService.Infrastructure.Data;
using PropostaService.Infrastructure.Repositories;
using PropostaService.Infrastructure.Services;
using Shared.Messaging.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("http://localhost:8080")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

builder.Services.AddSingleton<IPropostaRepository, PropostaRepository>();
builder.Services.AddScoped<IPropostaAppService, PropostaAppService>();
builder.Services.AddScoped<IValidationService, ValidationService>();
builder.Services.AddScoped<IDataSeeder, PropostaDataSeeder>();
builder.Services.AddScoped<IMediator, PropostaService.Infrastructure.Services.Mediator>();

builder.Services.AddScoped<PropostaService.Application.Handlers.ICommandHandler<PropostaService.Application.Commands.CriarPropostaCommand, PropostaService.Application.DTOs.PropostaResponse>, PropostaService.Application.Handlers.CriarPropostaCommandHandler>();
builder.Services.AddScoped<PropostaService.Application.Handlers.IQueryHandler<PropostaService.Application.Queries.ObterPropostaPorIdQuery, PropostaService.Application.DTOs.PropostaResponse?>, PropostaService.Application.Handlers.ObterPropostaPorIdQueryHandler>();
builder.Services.AddScoped<PropostaService.Application.Handlers.IQueryHandler<PropostaService.Application.Queries.ListarPropostasQuery, System.Collections.Generic.IEnumerable<PropostaService.Application.DTOs.PropostaResponse>>, PropostaService.Application.Handlers.ListarPropostasQueryHandler>();
builder.Services.AddScoped<PropostaService.Application.Handlers.ICommandHandler<PropostaService.Application.Commands.AlterarStatusPropostaCommand, PropostaService.Application.DTOs.PropostaResponse?>, PropostaService.Application.Handlers.AlterarStatusPropostaCommandHandler>();

builder.Services.AddHttpClient();

builder.Services.AddRabbitMqMessaging(builder.Configuration);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

if (app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

if (app.Environment.IsProduction())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowFrontend");

app.UseAuthorization();
app.MapControllers();

using (var scope = app.Services.CreateScope())
{
    var dataSeeder = scope.ServiceProvider.GetRequiredService<IDataSeeder>();
    await dataSeeder.SeedAsync();
}

app.Run();

public partial class Program { }
