using Microsoft.Extensions.DependencyInjection;

namespace MvvmScarletToolkit.Mediator
{
    public static class ServiceRegistrationExtensions
    {
        public static IServiceCollection RegisterSimpleMediator(this IServiceCollection services, SimpleMediatorRegistrator registrator)
        {
            var registry = registrator.Build();

            services.AddSingleton<ISimpleMediator, SimpleMediator>();
            services.AddSingleton(registry);

            foreach (var handler in registry.GetHandlers())
            {
                services.AddTransient(handler);
            }

            return services;
        }
    }
}