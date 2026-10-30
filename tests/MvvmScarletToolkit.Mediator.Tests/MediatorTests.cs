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
        registrator.RegisterNotification<NotificationClass, FirstNotificationHandler>();
        registrator.RegisterNotification<NotificationClass, SecondNotificationHandler>();
        registrator.RegisterNotification<NotificationStruct, NotificationStructHandler>();
        registrator.RegisterNotification<NotificationForAnyHandler, AnyNotificationHandler>();

        _services = new ServiceCollection();
        _services.AddSingleton<NotificationLog>();
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

    [Fact]
    public async Task Publish_Should_Invoke_All_Handlers_In_Registration_Order()
    {
        var mediator = _serviceProvider.GetRequiredService<ISimpleMediator>();
        await mediator.Publish(new NotificationClass("value"), TestContext.Current.CancellationToken);

        var log = _serviceProvider.GetRequiredService<NotificationLog>();
        Assert.Equal(["first:value", "second:value"], log.Entries);
    }

    [Fact]
    public async Task Publish_Should_Use_The_Runtime_Type_Of_The_Notification()
    {
        var mediator = _serviceProvider.GetRequiredService<ISimpleMediator>();
        ISimpleNotification notification = new NotificationClass("value");

        await mediator.Publish(notification, TestContext.Current.CancellationToken);

        var log = _serviceProvider.GetRequiredService<NotificationLog>();
        Assert.Equal(["first:value", "second:value"], log.Entries);
    }

    [Fact]
    public async Task Publish_Should_Support_Value_Types()
    {
        var mediator = _serviceProvider.GetRequiredService<ISimpleMediator>();
        await mediator.Publish(new NotificationStruct(true), TestContext.Current.CancellationToken);

        var log = _serviceProvider.GetRequiredService<NotificationLog>();
        Assert.Equal(["struct:True"], log.Entries);
    }

    [Fact]
    public async Task Publish_Should_Support_Handlers_For_All_Notifications()
    {
        var mediator = _serviceProvider.GetRequiredService<ISimpleMediator>();
        await mediator.Publish(new NotificationForAnyHandler(), TestContext.Current.CancellationToken);

        var log = _serviceProvider.GetRequiredService<NotificationLog>();
        Assert.Equal(["any"], log.Entries);
    }

    [Fact]
    public async Task Publish_Should_Throw_For_Unregistered_Notification()
    {
        var mediator = _serviceProvider.GetRequiredService<ISimpleMediator>();

        await Assert.ThrowsAsync<MissingNotificationRegistrationException>(() => mediator.Publish(new UnregisteredNotification(), TestContext.Current.CancellationToken));
    }

    public sealed class NotificationLog
    {
        public List<string> Entries { get; } = [];
    }

    public sealed record NotificationClass(string Value) : ISimpleNotification;
    public readonly record struct NotificationStruct(bool Value) : ISimpleNotification;
    public sealed record NotificationForAnyHandler : ISimpleNotification;
    public sealed record UnregisteredNotification : ISimpleNotification;

    public sealed class FirstNotificationHandler(NotificationLog log) : ISimpleNotificationHandler<NotificationClass>
    {
        public Task Handle(NotificationClass request, CancellationToken cancellationToken = default)
        {
            log.Entries.Add($"first:{request.Value}");

            return Task.CompletedTask;
        }
    }

    public sealed class SecondNotificationHandler(NotificationLog log) : ISimpleNotificationHandler<NotificationClass>
    {
        public Task Handle(NotificationClass request, CancellationToken cancellationToken = default)
        {
            log.Entries.Add($"second:{request.Value}");

            return Task.CompletedTask;
        }
    }

    public sealed class NotificationStructHandler(NotificationLog log) : ISimpleNotificationHandler<NotificationStruct>
    {
        public Task Handle(NotificationStruct request, CancellationToken cancellationToken = default)
        {
            log.Entries.Add($"struct:{request.Value}");

            return Task.CompletedTask;
        }
    }

    public sealed class AnyNotificationHandler(NotificationLog log) : ISimpleNotificationHandler<ISimpleNotification>
    {
        public Task Handle(ISimpleNotification request, CancellationToken cancellationToken = default)
        {
            log.Entries.Add("any");

            return Task.CompletedTask;
        }
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
