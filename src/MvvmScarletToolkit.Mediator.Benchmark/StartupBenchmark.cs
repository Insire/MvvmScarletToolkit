using BenchmarkDotNet.Attributes;
using Microsoft.Extensions.DependencyInjection;
using MvvmScarletToolkit.Mediator.Benchmark.Util;

namespace MvvmScarletToolkit.Mediator.Benchmark
{
    public class StartupBenchmark
    {
        [Benchmark(Baseline = true)]
        public ServiceProvider MediatRBuildServiceProvider()
        {
            var services = new ServiceCollection();
            services.AddMediatR(config => config.RegisterServicesFromAssembly(typeof(Program).Assembly));

            return services.BuildServiceProvider();
        }

        [Benchmark()]
        public ServiceProvider SimpleMediatorBuildServiceProvider()
        {
            var registrator = new SimpleMediatorRegistrator();
            registrator.RegisterRequest<RequestStruct, RequestStructHandler>();
            registrator.RegisterRequest<RequestStructWithResponse, ResponseStruct, RequestStructWithResponseHandler>();

            var services = new ServiceCollection();
            services.RegisterSimpleMediator(registrator);

            return services.BuildServiceProvider();
        }
    }
}
