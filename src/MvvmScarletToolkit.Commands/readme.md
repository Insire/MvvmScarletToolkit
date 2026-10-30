# MvvmScarletToolkit.Commands

![.NET standard 2.0](https://img.shields.io/badge/.NET-standard2.0-brightgreen)

## Goals

This library aims to provide easy to use [ICommand](https://docs.microsoft.com/en-gb/dotnet/api/system.windows.input.icommand) implementations for use in viewmodels.

## Features

- **Task based commands:** commands run a `Task`, stay responsive while it runs and expose `IsBusy`, `IsNotBusy` and `Completion` for binding.
- **Fluent configuration:** commands are created through `IScarletCommandBuilder.Create`, which accepts delegates with or without a `CancellationToken`, an argument and a `canExecute` check.
- **Cancellation:** `CancelCommand` can be bound to a cancel button, and the `CancellationToken` passed to your delegate is cancelled when it runs. Commands can only be cancelled when built with one of the cancellation options.
- **Single execution:** `WithSingleExecution` prevents a command from running again while it is busy. Without it, executing a busy command cancels the running execution and starts a new one.
- **Busy notifications:** `WithBusyNotification` reports execution to an `IBusyStack`, so a viewmodel can show one busy state for several commands.
- **Exception handling:** exceptions thrown by a command are passed to an `IScarletExceptionHandler`.

## Contents

|class|summary|
|---|---|
|``IScarletCommandBuilder``|Entry point for creating commands. It also exposes the dispatcher, command manager, messenger, exit service and weak event manager of the current UI framework.|
|``CommandBuilderContext<TArgument>``|Holds the configuration of a command, until ``Build`` creates it.|
|``ConcurrentCommand<TArgument>``|A Task based ``ICommand`` implementation supporting fluent configuration via ``IScarletCommandBuilder``.|
|``ConcurrentCommandBase``|Base class of all commands returned by ``Build``, exposing ``Completion``, ``CancelCommand``, ``IsBusy``, ``IsNotBusy`` and ``ExecuteAsync``.|
|``IgnoreExceptionHandler``|Default ``IScarletExceptionHandler``. It writes exceptions to the debug output in debug builds and otherwise ignores them.|

### Builder options

|method|summary|
|---|---|
|``WithSingleExecution()``|Only allow one execution at a time. An optional name identifies the command in debug output.|
|``WithBusyNotification(IBusyStack)``|Notify an ``IBusyStack`` while the command is running.|
|``WithCancellation()``|Synchronous cancellation through ``CancelCommand``.|
|``WithAsyncCancellation()``|Task based cancellation through ``CancelCommand``.|
|``WithCustomCancellation(ICancelCommand)``|Use your own ``ICancelCommand``. The instance is shared between all commands built from the same context.|

## Usage

### ConcurrentCommand\<TArgument>

```cs
public class SomeViewModel : ViewModelBase
{
    public ConcurrentCommandBase Command { get; }

    public SomeViewModel(IScarletCommandBuilder commandBuilder)
        : base(commandBuilder)
    {
        Command = commandBuilder
            .Create(Do, CanDo)
            .WithSingleExecution() // prevent from running multiple instances of this command at the same time
            .WithBusyNotification(BusyStack) // notify an IBusyStack instance that this command is running
            .WithAsyncCancellation() // use an async ICommand implementation for cancellation support
            .Build();
    }

    private Task Do(CancellationToken token)
    {
        return Task.Delay(2000, token);
    }

    private bool CanDo()
    {
        return true;
    }
}
```

Bind the command and its state in the view:

```xml
<Button Command="{Binding Command}" Content="Start" />
<Button Command="{Binding Command.CancelCommand}" Content="Cancel" />
<ProgressBar IsIndeterminate="{Binding Command.IsBusy}" />
```

### Commands with an argument

```cs
Command = commandBuilder
    .Create<string>(Save, CanSave)
    .WithSingleExecution()
    .Build();

private Task Save(string path, CancellationToken token)
{
    return File.WriteAllTextAsync(path, Content, token);
}

private bool CanSave(string path)
{
    return !string.IsNullOrEmpty(path);
}
```

### Handling exceptions

``ScarletCommandBuilder.Default`` uses ``IgnoreExceptionHandler``, so failures are not visible in release builds. To log them, implement ``IScarletExceptionHandler`` and pass it to the ``ScarletCommandBuilder`` constructor.

## Credits

``ConcurrentCommand<TArgument>`` is largely based on Stephen Clearlys blog post: [Async Programming : Patterns for Asynchronous MVVM Applications](https://msdn.microsoft.com/en-us/magazine/dn630647.aspx?f=255&MSPPError=-2147217396)
