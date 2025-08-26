using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using Shared.Messaging.Abstractions;
using Shared.Messaging.Commands;
using Shared.Messaging.Configuration;
using System.Text;
using System.Text.Json;

namespace Shared.Messaging.RabbitMq;

public class RabbitMqCommandPublisher : ICommandPublisher, IDisposable
{
    private readonly IConnection _connection;
    private readonly IModel _channel;
    private readonly RabbitMqSettings _settings;
    private readonly ILogger<RabbitMqCommandPublisher> _logger;

    public RabbitMqCommandPublisher(IOptions<RabbitMqSettings> settings, ILogger<RabbitMqCommandPublisher> logger)
    {
        _settings = settings.Value;
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
        _channel.QueueDeclare(_settings.CommandsQueueName, durable: true, exclusive: false, autoDelete: false);
        _channel.QueueBind(_settings.CommandsQueueName, _settings.ExchangeName, "commands.*");
    }

    public async Task PublishAsync<T>(T command) where T : ICommand
    {
        try
        {
            var message = JsonSerializer.Serialize(command);
            var body = Encoding.UTF8.GetBytes(message);

            var properties = _channel.CreateBasicProperties();
            properties.Persistent = true;
            properties.MessageId = command.Id.ToString();
            properties.Timestamp = new AmqpTimestamp(((DateTimeOffset)command.CreatedAt).ToUnixTimeSeconds());
            properties.Type = command.CommandType;

            var routingKey = $"commands.{command.CommandType}";

            _channel.BasicPublish(
                exchange: _settings.ExchangeName,
                routingKey: routingKey,
                basicProperties: properties,
                body: body);

            _logger.LogInformation("Command {CommandType} with ID {CommandId} published successfully", 
                command.CommandType, command.Id);

            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error publishing command {CommandType} with ID {CommandId}", 
                command.CommandType, command.Id);
            throw;
        }
    }

    public void Dispose()
    {
        _channel?.Dispose();
        _connection?.Dispose();
    }
}
