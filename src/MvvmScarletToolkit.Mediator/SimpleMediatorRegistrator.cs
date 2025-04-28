using System.Collections.Frozen;

namespace MvvmScarletToolkit.Mediator
{
    public sealed class SimpleMediatorRegistrator
    {
        private readonly Dictionary<Type, Type> _requestMap = new();
        private readonly Dictionary<Type, List<Type>> _notificationMap = new();

        public void RegisterRequest<TRequest, TRequestHandler>()
            where TRequest : ISimpleRequest
            where TRequestHandler : class, ISimpleRequestHandler<TRequest>
        {
            _requestMap[typeof(TRequest)] = typeof(TRequestHandler);
        }

        public void RegisterRequest<TRequest, TResponse, TRequestHandler>()
            where TRequest : ISimpleRequest<TResponse>
            where TRequestHandler : class, ISimpleRequestHandler<TRequest, TResponse>
        {
            _requestMap[typeof(TRequest)] = typeof(TRequestHandler);
        }

        public void RegisterNotification<TNotification, TNotificationHandler>()
            where TNotification : ISimpleNotification
            where TNotificationHandler : class, ISimpleNotificationHandler<TNotification>
        {
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

            return new SimpleMediatorRegistry(requestMap, notificationMap);
        }
    }
}
