using Microsoft.Extensions.DependencyInjection;
using System.Collections.Frozen;

namespace MvvmScarletToolkit.Mediator
{
    internal delegate Task NotificationPublisher(object? instance, Type handlerType, ISimpleNotification notification, CancellationToken cancellationToken);

    public sealed class SimpleMediatorRegistry
    {
        private readonly FrozenDictionary<Type, Type> _requestMap;
        private readonly FrozenDictionary<Type, IReadOnlyList<Type>> _notificationMap;
        private readonly FrozenDictionary<Type, NotificationPublisher> _notificationPublishers;
        private readonly IReadOnlyList<ServiceDescriptor> _handlerDescriptors;

        internal SimpleMediatorRegistry(
            FrozenDictionary<Type, Type> requestMap,
            FrozenDictionary<Type, IReadOnlyList<Type>> notificationMap,
            FrozenDictionary<Type, NotificationPublisher> notificationPublishers,
            IReadOnlyList<ServiceDescriptor> handlerDescriptors)
        {
            _requestMap = requestMap;
            _notificationMap = notificationMap;
            _notificationPublishers = notificationPublishers;
            _handlerDescriptors = handlerDescriptors;
        }

        public Type GetRequestHandlerType(ISimpleRequest request)
        {
            var requestType = request.GetType();
            if (!_requestMap.TryGetValue(requestType, out var handlerType))
            {
                throw new MissingRequestRegistrationException(requestType);
            }

            return handlerType;
        }

        public Type GetRequestHandlerType<TResponse>(ISimpleRequest<TResponse> request)
        {
            var requestType = request.GetType();
            if (!_requestMap.TryGetValue(requestType, out var handlerType))
            {
                throw new MissingRequestRegistrationException(requestType);
            }

            return handlerType;
        }

        public IReadOnlyList<Type> GetNotificationHandlerTypes(ISimpleNotification notification)
        {
            var notificationType = notification.GetType();
            if (!_notificationMap.TryGetValue(notificationType, out var handlerTypes))
            {
                throw new MissingNotificationRegistrationException(notificationType);
            }

            return handlerTypes;
        }

        internal NotificationPublisher GetNotificationPublisher(ISimpleNotification notification)
        {
            var notificationType = notification.GetType();
            if (!_notificationPublishers.TryGetValue(notificationType, out var publisher))
            {
                throw new MissingNotificationRegistrationException(notificationType);
            }

            return publisher;
        }

        /// <summary>
        /// Transient registrations of all handlers, created with their statically known types.
        /// </summary>
        internal IReadOnlyList<ServiceDescriptor> GetHandlerDescriptors()
        {
            return _handlerDescriptors;
        }
    }
}
