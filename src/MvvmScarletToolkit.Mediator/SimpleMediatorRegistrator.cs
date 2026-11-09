using Microsoft.Extensions.DependencyInjection;
using System.Collections.Frozen;
using System.Diagnostics.CodeAnalysis;

namespace MvvmScarletToolkit.Mediator
{
    public sealed class SimpleMediatorRegistrator
    {
        private readonly Dictionary<Type, Type> _requestMap = new();
        private readonly Dictionary<Type, List<Type>> _notificationMap = new();
        private readonly Dictionary<Type, NotificationPublisher> _notificationPublishers = new();

        // handler registrations are created where the handler type is known statically, which keeps them trim and AOT safe
        private readonly Dictionary<Type, ServiceDescriptor> _requestHandlerDescriptors = new();
        private readonly List<ServiceDescriptor> _notificationHandlerDescriptors = new();

        public void RegisterRequest<TRequest, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TRequestHandler>()
            where TRequest : ISimpleRequest
            where TRequestHandler : class, ISimpleRequestHandler<TRequest>
        {
            _requestMap[typeof(TRequest)] = typeof(TRequestHandler);
            _requestHandlerDescriptors[typeof(TRequest)] = ServiceDescriptor.Transient<TRequestHandler, TRequestHandler>();
        }

        public void RegisterRequest<TRequest, TResponse, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TRequestHandler>()
            where TRequest : ISimpleRequest<TResponse>
            where TRequestHandler : class, ISimpleRequestHandler<TRequest, TResponse>
        {
            _requestMap[typeof(TRequest)] = typeof(TRequestHandler);
            _requestHandlerDescriptors[typeof(TRequest)] = ServiceDescriptor.Transient<TRequestHandler, TRequestHandler>();
        }

        public void RegisterNotification<TNotification, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TNotificationHandler>()
            where TNotification : ISimpleNotification
            where TNotificationHandler : class, ISimpleNotificationHandler<TNotification>
        {
            // the notification type is only known here, the mediator receives notifications as ISimpleNotification
            _notificationPublishers[typeof(TNotification)] = PublishTo<TNotification>;
            _notificationHandlerDescriptors.Add(ServiceDescriptor.Transient<TNotificationHandler, TNotificationHandler>());

            if (_notificationMap.TryGetValue(typeof(TNotification), out var handlerTypes))
            {
                handlerTypes.Add(typeof(TNotificationHandler));
            }
            else
            {
                _notificationMap[typeof(TNotification)] = [typeof(TNotificationHandler)];
            }
        }

        public SimpleMediatorRegistry Build()
        {
            var requestMap = _requestMap.ToFrozenDictionary();
            var notificationMap = _notificationMap.ToFrozenDictionary(p => p.Key, IReadOnlyList<Type> (p) => p.Value);

            var notificationPublishers = _notificationPublishers.ToFrozenDictionary();

            ServiceDescriptor[] handlerDescriptors = [.. _requestHandlerDescriptors.Values, .. _notificationHandlerDescriptors];

            return new SimpleMediatorRegistry(requestMap, notificationMap, notificationPublishers, handlerDescriptors);
        }

        private static Task PublishTo<TNotification>(object? instance, Type handlerType, ISimpleNotification notification, CancellationToken cancellationToken)
            where TNotification : ISimpleNotification
        {
            var service = instance as ISimpleNotificationHandler<TNotification>;

            return service switch
            {
                null => throw new MissingHandlerRegistrationException(handlerType),
                _ => service.Handle((TNotification)notification, cancellationToken),
            };
        }
    }
}
