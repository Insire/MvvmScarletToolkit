# MvvmScarletToolkit.Messenger

![.NET standard 2.0](https://img.shields.io/badge/.NET-standard2.0-brightgreen)

## Goals

This library aims to provide a simple event aggregator implementations for the framework.

## Contents

|class|summary|
|---|---|
|[``ScarletMessageProxy``](./ScarletMessageProxy.cs)|``IScarletMessageProxy`` implementation used by the ``ScarletMessenger`` to forward and log Messages given to it. Logging is present in debug builds only.|
|[``ScarletMessenger``](./ScarletMessenger.cs)|A lightweight event aggregator/messenger for loosely coupled communication. (Modified version of [TinyMessenger](https://github.com/grumpydev/TinyMessenger)).|

## Usage

### Publish

```cs
using MvvmScarletToolkit.Abstractions;

// InterfaceDerivedMessage.cs
public class InterfaceDerivedMessage : IScarletMessage
{
    public object Sender { get; }

    public InterfaceDerivedMessage(object sender)
    {
        Sender = sender;
    }
}

// SomeOtherClass.cs
Messenger.Publish(new InterfaceDerivedMessage(this);
```

### Subscribe

```cs
// SomeClass.cs
Messenger.Subscribe<InterfaceDerivedMessage>(message =>
{
    // your code here
} ,(message)=>
{
    // filter here
     return true;
});
```

## Credits

``ScarletMessenger`` is largely based on [TinyMessenger](https://github.com/grumpydev/TinyMessenger), with a few tweaks here and there.
