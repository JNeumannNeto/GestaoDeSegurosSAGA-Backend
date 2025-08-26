using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Shared.Messaging.Abstractions;
using Shared.Messaging.Configuration;
using Shared.Messaging.RabbitMq;

namespace Shared.Messaging.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddRabbitMqMessaging(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<RabbitMqSettings>(options => configuration.GetSection("RabbitMq").Bind(options));
        
        services.AddSingleton<IEventPublisher, RabbitMqEventPublisher>();
        services.AddSingleton<ICommandPublisher, RabbitMqCommandPublisher>();
        
        return services;
    }

    public static IServiceCollection AddRabbitMqConsumer(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<RabbitMqSettings>(options => configuration.GetSection("RabbitMq").Bind(options));
        services.AddHostedService<RabbitMqConsumerImproved>();
        
        return services;
    }
}
