using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace MvvmScarletToolkit.Mediator.Benchmark.Util
{
    public sealed class MediatRRequests
    {
        private readonly IServiceCollection _services;
        private readonly ServiceProvider _serviceProvider;
        private readonly IMediator _mediatr;

        public MediatRRequests()
        {
            var registrator = new SimpleMediatorRegistrator();
            registrator.RegisterRequest<RequestStruct, RequestStructHandler>();
            registrator.RegisterRequest<RequestStructWithResponse, ResponseStruct, RequestStructWithResponseHandler>();

            _services = new ServiceCollection();
            _services.AddMediatR(config =>

                config.RegisterServicesFromAssembly(typeof(Program).Assembly)
            );

            _serviceProvider = _services.BuildServiceProvider();
            _mediatr = _serviceProvider.GetRequiredService<IMediator>();
        }

        public async Task Send_Request_With_Response()
        {
            var response = await _mediatr.Send(new RequestStructWithResponse(true), CancellationToken.None);
        }

        public async Task Send_Request()
        {
            await _mediatr.Send(new RequestStruct(true), CancellationToken.None);
        }
    }
}
