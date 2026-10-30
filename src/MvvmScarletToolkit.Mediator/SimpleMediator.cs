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
            var handlerTypes = _registry.GetNotificationHandlerTypes(notification);
            var publisher = _registry.GetNotificationPublisher(notification);

            foreach (var handlerType in handlerTypes)
            {
                var instance = _serviceProvider.GetService(handlerType);

                await publisher(instance, handlerType, notification, cancellationToken);
            }
        }
    }
}
