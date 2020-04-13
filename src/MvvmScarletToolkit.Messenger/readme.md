# MvvmScarletToolkit.Messenger

## Goals

This library aims to provide a simple event aggregator implementations for the framework.

## Contents

|class|summary|
|---|---|
|[``ScarletMessageProxy``](.\ScarletMessageProxy.cs)|``IScarletMessageProxy`` implementation used by the ``ScarletMessenger`` to forward and log Messages given to it. Logging is present in debug builds only.|
|[``ScarletMessenger``](.\ScarletMessenger.cs)|A lightweight event aggregator/messenger for loosely coupled communication. (Modified version of [Source](https://github.com/grumpydev/TinyMessenger)).|

## Credits

``ScarletMessenger`` is largely based on [TinyMessenger](https://github.com/grumpydev/TinyMessenger), with a few tweaks here and there.
