using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Shared.Messaging.Abstractions;
using Shared.Messaging.Commands;
using Shared.Messaging.Configuration;
using Shared.Messaging.Events;
using System.Text;
using System.Text.Json;

namespace Shared.Messaging.RabbitMq;

public class RabbitMqConsumer : BackgroundService
{
    private readonly IConnection _connection;
    private readonly IModel _channel;
    private readonly RabbitMqSettings _settings;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<RabbitMqConsumer> _logger;

    public RabbitMqConsumer(
        IOptions<RabbitMqSettings> settings,
        IServiceProvider serviceProvider,
        ILogger<RabbitMqConsumer> logger)
    {
        _settings = settings.Value;
        _serviceProvider = serviceProvider;
        _logger = logger;

        var factory = new ConnectionFactory
        {
            HostName = _settings.HostName,
            Port = _settings.Port,
            UserName = _settings.UserName,
            Password = _settings.Password,
            VirtualHost = _settings.VirtualHost
        };

        _connection = factory.CreateConnection();
        _channel = _connection.CreateModel();

        _channel.ExchangeDeclare(_settings.ExchangeName, ExchangeType.Topic, durable: true);
        _channel.QueueDeclare(_settings.EventsQueueName, durable: true, exclusive: false, autoDelete: false);
        _channel.QueueDeclare(_settings.CommandsQueueName, durable: true, exclusive: false, autoDelete: false);
        _channel.QueueBind(_settings.EventsQueueName, _settings.ExchangeName, "events.*");
        _channel.QueueBind(_settings.CommandsQueueName, _settings.ExchangeName, "commands.*");
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var eventConsumer = new EventingBasicConsumer(_channel);
        var commandConsumer = new EventingBasicConsumer(_channel);

        eventConsumer.Received += async (model, ea) =>
        {
            await ProcessEventMessage(ea);
        };

        commandConsumer.Received += async (model, ea) =>
        {
            await ProcessCommandMessage(ea);
        };

        _channel.BasicConsume(queue: _settings.EventsQueueName, autoAck: false, consumer: eventConsumer);
        _channel.BasicConsume(queue: _settings.CommandsQueueName, autoAck: false, consumer: commandConsumer);

        _logger.LogInformation("RabbitMQ Consumer started");

        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(1000, stoppingToken);
        }
    }

    private async Task ProcessEventMessage(BasicDeliverEventArgs ea)
    {
        try
        {
            var body = ea.Body.ToArray();
            var message = Encoding.UTF8.GetString(body);
            var eventType = ea.BasicProperties.Type;

            _logger.LogInformation("Processing event: {EventType}", eventType);

            using var scope = _serviceProvider.CreateScope();

            switch (eventType)
            {
                case "PropostaStatusAlterada":
                    var propostaEvent = JsonSerializer.Deserialize<PropostaStatusAlteradaEvent>(message);
                    if (propostaEvent != null)
                    {
                        var handlers = scope.ServiceProvider.GetServices<IEventHandler<PropostaStatusAlteradaEvent>>();
                        foreach (var handler in handlers)
                        {
                            await handler.HandleAsync(propostaEvent);
                        }
                    }
                    break;

                case "ContratacaoProcessada":
                    var contratacaoEvent = JsonSerializer.Deserialize<ContratacaoProcessadaEvent>(message);
                    if (contratacaoEvent != null)
                    {
                        var handlers = scope.ServiceProvider.GetServices<IEventHandler<ContratacaoProcessadaEvent>>();
                        foreach (var handler in handlers)
                        {
                            await handler.HandleAsync(contratacaoEvent);
                        }
                    }
                    break;
            }

            _channel.BasicAck(ea.DeliveryTag, false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing event message");
            _channel.BasicNack(ea.DeliveryTag, false, true);
        }
    }

    private async Task ProcessCommandMessage(BasicDeliverEventArgs ea)
    {
        try
        {
            var body = ea.Body.ToArray();
            var message = Encoding.UTF8.GetString(body);
            var commandType = ea.BasicProperties.Type;

            _logger.LogInformation("Processing command: {CommandType}", commandType);

            using var scope = _serviceProvider.CreateScope();

            switch (commandType)
            {
                case "ContratarProposta":
                    var command = JsonSerializer.Deserialize<ContratarPropostaCommand>(message);
                    if (command != null)
                    {
                        var handler = scope.ServiceProvider.GetService<ICommandHandler<ContratarPropostaCommand>>();
                        if (handler != null)
                        {
                            await handler.HandleAsync(command);
                        }
                    }
                    break;
            }

            _channel.BasicAck(ea.DeliveryTag, false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing command message");
            _channel.BasicNack(ea.DeliveryTag, false, true);
        }
    }

    public override void Dispose()
    {
        _channel?.Dispose();
        _connection?.Dispose();
        base.Dispose();
    }
}
