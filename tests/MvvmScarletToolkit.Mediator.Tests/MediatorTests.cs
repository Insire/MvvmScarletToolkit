using Microsoft.Extensions.DependencyInjection;

namespace MvvmScarletToolkit.Mediator.Tests;

public sealed class MediatorTests : TraceTestBase, IAsyncLifetime
{
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    private IServiceCollection _services;
    private ServiceProvider _serviceProvider;
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

    public ValueTask InitializeAsync()
    {
        var registrator = new SimpleMediatorRegistrator();
        registrator.RegisterRequest<RequestStruct, RequestStructHandler>();
        registrator.RegisterRequest<RequestStructWithResponse, ResponseStruct, RequestStructWithResponseHandler>();

        _services = new ServiceCollection();
        _services.RegisterSimpleMediator(registrator);

        _serviceProvider = _services.BuildServiceProvider();

        return ValueTask.CompletedTask;
    }

    public ValueTask DisposeAsync()
    {
        _serviceProvider.Dispose();

        return ValueTask.CompletedTask;
    }

    [Fact]
    public async Task Send_Request_Should_Return_Expected_Reponse()
    {
        var mediator = _serviceProvider.GetRequiredService<ISimpleMediator>();
        var response = await mediator.Send<RequestStructWithResponse, ResponseStruct>(new RequestStructWithResponse(true), TestContext.Current.CancellationToken);

        Assert.True(response.Value);
    }

    [Fact]
    public async Task Send_Request_Should_Handle()
    {
        var mediator = _serviceProvider.GetRequiredService<ISimpleMediator>();
        await mediator.Send(new RequestStruct(true), TestContext.Current.CancellationToken);
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
