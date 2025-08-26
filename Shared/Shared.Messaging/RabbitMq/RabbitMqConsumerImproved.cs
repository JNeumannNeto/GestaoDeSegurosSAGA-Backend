using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.Exceptions;
using Shared.Messaging.Abstractions;
using Shared.Messaging.Commands;
using Shared.Messaging.Configuration;
using Shared.Messaging.Events;
using System.Text;
using System.Text.Json;

namespace Shared.Messaging.RabbitMq;

public class RabbitMqConsumerImproved : BackgroundService
{
    private IConnection? _connection;
    private IModel? _channel;
    private readonly RabbitMqSettings _settings;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<RabbitMqConsumerImproved> _logger;
    private readonly int _maxRetryAttempts = 10;
    private readonly TimeSpan _retryDelay = TimeSpan.FromSeconds(5);

    public RabbitMqConsumerImproved(
        IOptions<RabbitMqSettings> settings,
        IServiceProvider serviceProvider,
        ILogger<RabbitMqConsumerImproved> logger)
    {
        _settings = settings.Value;
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await ConnectWithRetry(stoppingToken);

        if (_connection == null || _channel == null)
        {
            _logger.LogError("Failed to establish RabbitMQ connection after all retry attempts");
            return;
        }

        SetupQueuesAndExchanges();
        SetupConsumers();

        _logger.LogInformation("RabbitMQ Consumer started successfully");

        while (!stoppingToken.IsCancellationRequested)
        {
            if (_connection?.IsOpen != true)
            {
                _logger.LogWarning("RabbitMQ connection lost, attempting to reconnect...");
                await ConnectWithRetry(stoppingToken);
                if (_connection?.IsOpen == true)
                {
                    SetupQueuesAndExchanges();
                    SetupConsumers();
                }
            }

            await Task.Delay(5000, stoppingToken);
        }
    }

    private async Task ConnectWithRetry(CancellationToken stoppingToken)
    {
        var attempt = 0;
        while (attempt < _maxRetryAttempts && !stoppingToken.IsCancellationRequested)
        {
            try
            {
                _logger.LogInformation("Attempting to connect to RabbitMQ (attempt {Attempt}/{MaxAttempts})", 
                    attempt + 1, _maxRetryAttempts);

                var factory = new ConnectionFactory
                {
                    HostName = _settings.HostName,
                    Port = _settings.Port,
                    UserName = _settings.UserName,
                    Password = _settings.Password,
                    VirtualHost = _settings.VirtualHost,
                    RequestedConnectionTimeout = TimeSpan.FromSeconds(30),
                    RequestedHeartbeat = TimeSpan.FromSeconds(60),
                    AutomaticRecoveryEnabled = true,
                    NetworkRecoveryInterval = TimeSpan.FromSeconds(10)
                };

                _connection?.Dispose();
                _channel?.Dispose();

                _connection = factory.CreateConnection();
                _channel = _connection.CreateModel();

                _logger.LogInformation("Successfully connected to RabbitMQ");
                return;
            }
            catch (BrokerUnreachableException ex)
            {
                attempt++;
                _logger.LogWarning(ex, "Failed to connect to RabbitMQ (attempt {Attempt}/{MaxAttempts}). Retrying in {Delay} seconds...", 
                    attempt, _maxRetryAttempts, _retryDelay.TotalSeconds);

                if (attempt < _maxRetryAttempts)
                {
                    await Task.Delay(_retryDelay, stoppingToken);
                }
            }
            catch (Exception ex)
            {
                attempt++;
                _logger.LogError(ex, "Unexpected error connecting to RabbitMQ (attempt {Attempt}/{MaxAttempts})", 
                    attempt, _maxRetryAttempts);

                if (attempt < _maxRetryAttempts)
                {
                    await Task.Delay(_retryDelay, stoppingToken);
                }
            }
        }
    }

    private void SetupQueuesAndExchanges()
    {
        if (_channel == null) return;

        try
        {
            _channel.ExchangeDeclare(_settings.ExchangeName, ExchangeType.Topic, durable: true);
            _channel.QueueDeclare(_settings.EventsQueueName, durable: true, exclusive: false, autoDelete: false);
            _channel.QueueDeclare(_settings.CommandsQueueName, durable: true, exclusive: false, autoDelete: false);
            _channel.QueueBind(_settings.EventsQueueName, _settings.ExchangeName, "events.*");
            _channel.QueueBind(_settings.CommandsQueueName, _settings.ExchangeName, "commands.*");

            _logger.LogInformation("RabbitMQ queues and exchanges setup completed");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting up RabbitMQ queues and exchanges");
            throw;
        }
    }

    private void SetupConsumers()
    {
        if (_channel == null) return;

        try
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

            _logger.LogInformation("RabbitMQ consumers setup completed");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting up RabbitMQ consumers");
            throw;
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

            _channel?.BasicAck(ea.DeliveryTag, false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing event message");
            _channel?.BasicNack(ea.DeliveryTag, false, true);
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

            _channel?.BasicAck(ea.DeliveryTag, false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing command message");
            _channel?.BasicNack(ea.DeliveryTag, false, true);
        }
    }

    public override void Dispose()
    {
        try
        {
            _channel?.Close();
            _connection?.Close();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error closing RabbitMQ connection");
        }
        finally
        {
            _channel?.Dispose();
            _connection?.Dispose();
            base.Dispose();
        }
    }
}
