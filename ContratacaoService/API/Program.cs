using ContratacaoService.API.Middleware;
using ContratacaoService.Application.Handlers;
using ContratacaoService.Application.Sagas.ContratarPropostaSaga;
using ContratacaoService.Application.Sagas.ContratarPropostaSaga.Handlers;
using ContratacaoService.Application.Sagas.ContratarPropostaSaga.Steps;
using ContratacaoService.Application.Services;
using ContratacaoService.Domain.Ports;
using ContratacaoService.Infrastructure.Clients;
using ContratacaoService.Infrastructure.Repositories;
using ContratacaoService.Infrastructure.Services;
using Shared.Messaging.Abstractions;
using Shared.Messaging.Commands;
using Shared.Messaging.Extensions;
using Shared.Saga.Extensions;

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

builder.Services.AddScoped<IContratacaoRepository, ContratacaoRepository>();
builder.Services.AddScoped<IValidationService, ValidationService>();
builder.Services.AddScoped<ContratacaoAppService>();

builder.Services.AddHttpClient<IPropostaServiceClient, PropostaServiceClient>(client =>
{
    var baseUrl = builder.Environment.IsProduction() 
        ? "http://proposta-service-saga:8080/" 
        : "https://localhost:7001/";
    client.BaseAddress = new Uri(baseUrl);
    client.Timeout = TimeSpan.FromSeconds(30);
});

builder.Services.AddRabbitMqMessaging(builder.Configuration);
builder.Services.AddRabbitMqConsumer(builder.Configuration);

// SAGA Infrastructure
builder.Services.AddSaga();

// SAGA Steps
builder.Services.AddScoped<ValidarPropostaStep>();
builder.Services.AddScoped<VerificarDisponibilidadeStep>();
builder.Services.AddScoped<CriarContratacaoStep>();
builder.Services.AddScoped<PublicarEventoStep>();

// SAGA Implementation
builder.Services.AddScoped<ContratarPropostaSaga>();

// SAGA Handler
builder.Services.AddScoped<ContratarPropostaSagaHandler>();

// Command Handlers
builder.Services.AddScoped<ICommandHandler<ContratarPropostaCommand>, ContratarPropostaSagaHandler>();

builder.Services.AddScoped<Shared.Messaging.Abstractions.IEventHandler<Shared.Messaging.Events.PropostaStatusAlteradaEvent>, Shared.Messaging.Handlers.NotificacaoEventHandler>();
builder.Services.AddScoped<Shared.Messaging.Abstractions.IEventHandler<Shared.Messaging.Events.ContratacaoProcessadaEvent>, Shared.Messaging.Handlers.NotificacaoEventHandler>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

if (app.Environment.IsProduction())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseMiddleware<GlobalExceptionMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseCors("AllowFrontend");

app.UseAuthorization();
app.MapControllers();

app.Run();
