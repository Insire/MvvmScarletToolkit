using Microsoft.Extensions.DependencyInjection;

namespace MvvmScarletToolkit.Mediator.Benchmark.Util
{
    public sealed class SimpleMediatorRequests
    {
        private readonly IServiceCollection _services;
        private readonly ServiceProvider _serviceProvider;
        private readonly ISimpleMediator _simpleMediator;

        public SimpleMediatorRequests()
        {
            var registrator = new SimpleMediatorRegistrator();
            registrator.RegisterRequest<RequestStruct, RequestStructHandler>();
            registrator.RegisterRequest<RequestStructWithResponse, ResponseStruct, RequestStructWithResponseHandler>();

            _services = new ServiceCollection();
            _services.RegisterSimpleMediator(registrator);

            _serviceProvider = _services.BuildServiceProvider();

            _simpleMediator = _serviceProvider.GetRequiredService<ISimpleMediator>();
        }

        public void Dispose()
        {
            _serviceProvider.Dispose();
        }

        public async Task Send_Request_With_Response()
        {
            var response = await _simpleMediator.Send<RequestStructWithResponse, ResponseStruct>(new RequestStructWithResponse(true), CancellationToken.None);
        }

        public async Task Send_Request()
        {
            await _simpleMediator.Send(new RequestStruct(true), CancellationToken.None);
        }
    }
}
