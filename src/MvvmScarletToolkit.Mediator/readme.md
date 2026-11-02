# MvvmScarletToolkit.Mediator

## Goals

MvvmScarletToolkit.Mediator is part of the MvvmScarletToolkit framework, containing a simple implementation of the mediator pattern.

## Features

- **Requests:** send a request to its single handler, with or without a response.
- **Notifications:** publish a notification to any number of handlers, one after another.
- **Explicit registration:** requests and handlers are registered in code through `SimpleMediatorRegistrator`, without assembly scanning.
- **Dependency injection:** `RegisterSimpleMediator` adds the mediator as a singleton to an `IServiceCollection`, and its handlers as transient services.

## Contents

|class|summary|
|---|---|
|``ISimpleMediator``|Combines ``ISimpleSender`` and ``ISimplePublisher``. Inject this to send requests and publish notifications.|
|``ISimpleRequest``, ``ISimpleRequest<TResponse>``|Marker interfaces for requests without and with a response.|
|``ISimpleRequestHandler<TRequest>``, ``ISimpleRequestHandler<TRequest, TResponse>``|Handle a request.|
|``ISimpleNotification``|Marker interface for notifications.|
|``ISimpleNotificationHandler<TNotification>``|Handles a notification.|
|``SimpleMediatorRegistrator``|Collects the registrations of requests and notifications.|
|``MissingRequestRegistrationException``, ``MissingNotificationRegistrationException``|Thrown when sending or publishing something that has not been registered.|
|``MissingHandlerRegistrationException``|Thrown when a registered handler can not be resolved.|

## Usage

Define a request and its handler:

```cs
public sealed record GetGreeting(string Name) : ISimpleRequest<string>;

public sealed class GetGreetingHandler : ISimpleRequestHandler<GetGreeting, string>
{
    public Task<string> Handle(GetGreeting request, CancellationToken cancellationToken = default)
    {
        return Task.FromResult($"Hello {request.Name}");
    }
}
```

Register them:

```cs
var registrator = new SimpleMediatorRegistrator();
registrator.RegisterRequest<GetGreeting, string, GetGreetingHandler>();

services.RegisterSimpleMediator(registrator);
```

Send the request through `ISimpleMediator`. Requests with a response need both type arguments:

```cs
var greeting = await mediator.Send<GetGreeting, string>(new GetGreeting("World"), token);
```

Requests without a response use `Send(request, token)`.

### Notifications

```cs
public sealed record UserLoggedIn(string Name) : ISimpleNotification;

public sealed class UserLoggedInHandler : ISimpleNotificationHandler<UserLoggedIn>
{
    public Task Handle(UserLoggedIn request, CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }
}
```

```cs
registrator.RegisterNotification<UserLoggedIn, UserLoggedInHandler>();

await mediator.Publish(new UserLoggedIn("World"), token);
```

Handlers run one after another, in the order they were registered. A handler can also implement `ISimpleNotificationHandler<ISimpleNotification>`, to be registered for several notification types.
