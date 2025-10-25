using System;
using System.ComponentModel;
using System.Windows;

namespace MvvmScarletToolkit
{
    public static class IoCRegistrationExtensions
    {
        private static readonly Type _serviceProviderTypeInstance = typeof(IServiceProvider);

        public static void Register(this ResourceDictionary dictionary, IContainer container)
        {
            dictionary.Add(_serviceProviderTypeInstance, container);
        }

        public static T GetService<T>(this FrameworkElement control)
        {
            var container = control.FindResource(_serviceProviderTypeInstance);
            if (container is IServiceProvider provider)
            {
                return provider.GetRequiredService<T>();
            }

            throw new InvalidOperationException("ServiceProvider not found in resources");
        }

        private static T GetRequiredService<T>(this IServiceProvider provider)
        {
            var serviceType = typeof(T);
            var service = provider.GetService(serviceType);
            if (service is null)
            {
                throw new InvalidOperationException($"No service registration found for {serviceType.FullName}");
            }

            if (service is not T instance)
            {
                throw new InvalidOperationException($"IServiceProvider returned invalid type of {service.GetType()} when asked for {serviceType.FullName}");
            }

            return instance;
        }
    }
}
