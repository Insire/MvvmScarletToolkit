namespace MvvmScarletToolkit.Mediator
{
    public sealed class SimpleMediator(IServiceProvider serviceProvider, SimpleMediatorRegistry registry) : ISimpleMediator
    {
        private readonly IServiceProvider _serviceProvider = serviceProvider;
        private readonly SimpleMediatorRegistry _registry = registry;

        public Task Send<TRequest>(TRequest request, CancellationToken cancellationToken = default)
            where TRequest : ISimpleRequest
        {
            var handlerType = _registry.GetRequestHandlerType(request);
            var instance = _serviceProvider.GetService(handlerType);
            var service = instance as ISimpleRequestHandler<TRequest>;

            return service switch
            {
                null => throw new MissingHandlerRegistrationException(handlerType),
                _ => service.Handle(request, cancellationToken)
            };
        }

        public Task<TResponse> Send<TRequest, TResponse>(TRequest request, CancellationToken cancellationToken = default)
            where TRequest : ISimpleRequest<TResponse>
        {
            var handlerType = _registry.GetRequestHandlerType(request);
            var instance = _serviceProvider.GetService(handlerType);
            var service = instance as ISimpleRequestHandler<TRequest, TResponse>;

            return service switch
            {
                null => throw new MissingHandlerRegistrationException(handlerType),
                _ => service.Handle(request, cancellationToken),
            };
        }

        public async Task Publish(ISimpleNotification notification, CancellationToken cancellationToken = default)
        {
            foreach (var handlerType in _registry.GetNotificationHandlerTypes(notification))
            {
                var instance = _serviceProvider.GetService(handlerType);

                if (instance is not ISimpleNotificationHandler<ISimpleNotification> service)
                {
                    throw new MissingHandlerRegistrationException(handlerType);
                }

                await service.Handle(notification, cancellationToken);
            }
        }
    }
}
