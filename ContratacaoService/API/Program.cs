using ContratacaoService.API.Middleware;
using ContratacaoService.Application.Handlers;
using ContratacaoService.Application.Services;
using ContratacaoService.Domain.Ports;
using ContratacaoService.Infrastructure.Clients;
using ContratacaoService.Infrastructure.Repositories;
using ContratacaoService.Infrastructure.Services;
using Shared.Messaging.Abstractions;
using Shared.Messaging.Commands;
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

builder.Services.AddScoped<IContratacaoRepository, ContratacaoRepository>();
builder.Services.AddScoped<IValidationService, ValidationService>();
builder.Services.AddScoped<ContratacaoAppService>();

builder.Services.AddHttpClient<IPropostaServiceClient, PropostaServiceClient>(client =>
{
    client.BaseAddress = new Uri("https://localhost:7001/");
    client.Timeout = TimeSpan.FromSeconds(30);
});

builder.Services.AddRabbitMqMessaging(builder.Configuration);
builder.Services.AddRabbitMqConsumer(builder.Configuration);

builder.Services.AddScoped<ICommandHandler<ContratarPropostaCommand>, ContratarPropostaCommandHandler>();

builder.Services.AddScoped<Shared.Messaging.Abstractions.IEventHandler<Shared.Messaging.Events.PropostaStatusAlteradaEvent>, Shared.Messaging.Handlers.NotificacaoEventHandler>();
builder.Services.AddScoped<Shared.Messaging.Abstractions.IEventHandler<Shared.Messaging.Events.ContratacaoProcessadaEvent>, Shared.Messaging.Handlers.NotificacaoEventHandler>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseMiddleware<GlobalExceptionMiddleware>();

app.UseHttpsRedirection();

app.UseCors("AllowFrontend");

app.UseAuthorization();
app.MapControllers();

app.Run();
