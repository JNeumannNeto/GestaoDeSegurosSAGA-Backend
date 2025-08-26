namespace Shared.Messaging.Configuration;

public class RabbitMqSettings
{
    public string HostName { get; set; } = "localhost";
    public int Port { get; set; } = 5672;
    public string UserName { get; set; } = "guest";
    public string Password { get; set; } = "guest";
    public string VirtualHost { get; set; } = "/";
    public string ExchangeName { get; set; } = "seguros.exchange";
    public string EventsQueueName { get; set; } = "seguros.events";
    public string CommandsQueueName { get; set; } = "seguros.commands";
}
