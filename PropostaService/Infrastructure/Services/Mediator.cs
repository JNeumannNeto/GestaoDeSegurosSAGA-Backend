using Microsoft.Extensions.DependencyInjection;
using PropostaService.Application.Commands;
using PropostaService.Application.Handlers;
using PropostaService.Application.Queries;
using PropostaService.Application.Services;

namespace PropostaService.Infrastructure.Services;

public class Mediator : IMediator
{
    private readonly IServiceProvider _serviceProvider;

    public Mediator(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
    }

    public async Task<TResponse> SendAsync<TResponse>(ICommand<TResponse> command)
    {
        var handlerType = typeof(ICommandHandler<,>).MakeGenericType(command.GetType(), typeof(TResponse));
        var handler = _serviceProvider.GetRequiredService(handlerType);
        
        var method = handlerType.GetMethod("HandleAsync");
        var result = method?.Invoke(handler, new object[] { command });
        
        if (result is Task<TResponse> task)
            return await task;
            
        throw new InvalidOperationException($"Handler for {command.GetType().Name} returned invalid result");
    }

    public async Task<TResponse> SendAsync<TResponse>(IQuery<TResponse> query)
    {
        var handlerType = typeof(IQueryHandler<,>).MakeGenericType(query.GetType(), typeof(TResponse));
        var handler = _serviceProvider.GetRequiredService(handlerType);
        
        var method = handlerType.GetMethod("HandleAsync");
        var result = method?.Invoke(handler, new object[] { query });
        
        if (result is Task<TResponse> task)
            return await task;
            
        throw new InvalidOperationException($"Handler for {query.GetType().Name} returned invalid result");
    }
}
