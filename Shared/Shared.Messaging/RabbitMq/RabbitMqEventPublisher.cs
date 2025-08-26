using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using Shared.Messaging.Abstractions;
using Shared.Messaging.Configuration;
using Shared.Messaging.Events;
using System.Text;
using System.Text.Json;

namespace Shared.Messaging.RabbitMq;

public class RabbitMqEventPublisher : IEventPublisher, IDisposable
{
    private readonly IConnection _connection;
    private readonly IModel _channel;
    private readonly RabbitMqSettings _settings;
    private readonly ILogger<RabbitMqEventPublisher> _logger;

    public RabbitMqEventPublisher(IOptions<RabbitMqSettings> settings, ILogger<RabbitMqEventPublisher> logger)
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
        _channel.QueueDeclare(_settings.EventsQueueName, durable: true, exclusive: false, autoDelete: false);
        _channel.QueueBind(_settings.EventsQueueName, _settings.ExchangeName, "events.*");
    }

    public async Task PublishAsync<T>(T @event) where T : IEvent
    {
        try
        {
            var message = JsonSerializer.Serialize(@event);
            var body = Encoding.UTF8.GetBytes(message);

            var properties = _channel.CreateBasicProperties();
            properties.Persistent = true;
            properties.MessageId = @event.Id.ToString();
            properties.Timestamp = new AmqpTimestamp(((DateTimeOffset)@event.OccurredAt).ToUnixTimeSeconds());
            properties.Type = @event.EventType;

            var routingKey = $"events.{@event.EventType}";

            _channel.BasicPublish(
                exchange: _settings.ExchangeName,
                routingKey: routingKey,
                basicProperties: properties,
                body: body);

            _logger.LogInformation("Event {EventType} with ID {EventId} published successfully", 
                @event.EventType, @event.Id);

            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error publishing event {EventType} with ID {EventId}", 
                @event.EventType, @event.Id);
            throw;
        }
    }

    public void Dispose()
    {
        _channel?.Dispose();
        _connection?.Dispose();
    }
}
