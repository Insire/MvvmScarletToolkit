using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace MvvmScarletToolkit.Mediator.Tests;

public sealed class MediatorTests : IDisposable
{
    private IServiceCollection _services;
    private ServiceProvider _serviceProvider;

    [SetUp]
    public void Setup()
    {
        var registrator = new SimpleMediatorRegistrator();
        registrator.RegisterRequest<RequestStruct, RequestStructHandler>();
        registrator.RegisterRequest<RequestStructWithResponse, ResponseStruct, RequestStructWithResponseHandler>();

        _services = new ServiceCollection();
        _services.RegisterSimpleMediator(registrator);

        _serviceProvider = _services.BuildServiceProvider();
    }

    [TearDown]
    public void Dispose()
    {
        _serviceProvider.Dispose();
    }

    [Test]
    public async Task Send_Request_Should_Return_Expected_Reponse()
    {
        var mediator = _serviceProvider.GetRequiredService<ISimpleMediator>();
        var response = await mediator.Send<RequestStructWithResponse, ResponseStruct>(new RequestStructWithResponse(true));

        Assert.That(response.Value, Is.True);
    }

    [Test]
    public async Task Send_Request_Should_Handle()
    {
        var mediator = _serviceProvider.GetRequiredService<ISimpleMediator>();
        await mediator.Send(new RequestStruct(true));
    }

    public readonly record struct RequestStruct(bool Value) : ISimpleRequest;

    public sealed class RequestStructHandler : ISimpleRequestHandler<RequestStruct>
    {
        public async Task Handle(RequestStruct request, CancellationToken cancellationToken = default)
        {
            await ValueTask.CompletedTask;
        }
    }

    public readonly record struct RequestStructWithResponse(bool Value) : ISimpleRequest<ResponseStruct>;
    public readonly record struct ResponseStruct(bool Value);

    public sealed class RequestStructWithResponseHandler : ISimpleRequestHandler<RequestStructWithResponse, ResponseStruct>
    {
        public async Task<ResponseStruct> Handle(RequestStructWithResponse request, CancellationToken cancellationToken = default)
        {
            await ValueTask.CompletedTask;

            return new ResponseStruct(request.Value);
        }
    }
}
