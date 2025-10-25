using System.Collections.Frozen;

namespace MvvmScarletToolkit.Mediator
{
    public sealed class SimpleMediatorRegistry(FrozenDictionary<Type, Type> requestMap, FrozenDictionary<Type, IReadOnlyList<Type>> notificationMap)
    {
        private readonly FrozenDictionary<Type, Type> _requestMap = requestMap;
        private readonly FrozenDictionary<Type, IReadOnlyList<Type>> _notificationMap = notificationMap;

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

        internal IEnumerable<Type> GetHandlers()
        {
            foreach (var handlerType in _requestMap.Values)
            {
                yield return handlerType;
            }

            foreach (var handlerTypes in _notificationMap.Values)
            {
                foreach (var handlerType in handlerTypes)
                {
                    yield return handlerType;
                }
            }
        }
    }
}
