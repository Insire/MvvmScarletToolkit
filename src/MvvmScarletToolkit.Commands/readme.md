# MvvmScarletToolkit.Commands

![.NET standard 2.0](https://img.shields.io/badge/.NET-standard2.0-brightgreen)

## Goals

This library aims to provide easy to use [ICommand](https://docs.microsoft.com/en-gb/dotnet/api/system.windows.input.icommand) implementations for use in viewmodels.

## Contents

|class|summary|
|---|---|
|``ConcurrentCommand<TArgument>``|A Task based ``ICommand`` implementation supporting fluent configuration via ``IScarletCommandBuilder``.|

## Usage


### ConcurrentCommand\<TArgument>

```cs
public class SomeClass
{
    public ICommand Command { get; }

    public SomeClass(IScarletCommandBuilder commandBuilder)
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
        return Task.Delay(2000);
    }

    private bool CanDo()
    {
        return true;
    }
}
```


## Credits

``ConcurrentCommand<TArgument>`` is largely based on Stephen Clearlys blog post: [Async Programming : Patterns for Asynchronous MVVM Applications](https://msdn.microsoft.com/en-us/magazine/dn630647.aspx?f=255&MSPPError=-2147217396)
