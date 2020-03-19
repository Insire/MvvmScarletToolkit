# MvvmScarletToolkit.Implementations

## Goals

This library aims to provide implementations for wpf specific classes used by the framework.

## Contents

|class|summary|
|---|---|
|[``AsyncCommand``](.\AsyncCommand.cs)|Static factory for creating AsyncCommand instances.|
|[``ScarletCommandBuilder``](.\ScarletCommandBuilder.cs)|Fluent builder for ConcurrentCommand instances.|
|[``ScarletCommandManager``](.\ScarletCommandManager.cs)|Wrapper for [System.Windows.Input.CommandManager](https://docs.microsoft.com/en-gb/dotnet/api/system.windows.input.commandmanager) to support Dependency Injection.|
|[``ScarletDispatcher``](.\ScarletDispatcher.cs)|Wrapper for [System.Windows.Threading.Dispatcher](https://docs.microsoft.com/en-us/dotnet/api/system.windows.threading.dispatcher) to support Dependency Injection and Task based ICommand implementations in this library.|
|[``ScarletExitService``](.\ScarletExitService.cs)|In WPF ui controls dont always raise their Uunloaded-event, when the application is being closed. ScarletExitService takes care of executing ``IVirtualizationViewModel.UnloadCommand`` when the application exits.|
|[``ScarletLocalizationProvider``](.\ScarletLocalizationProvider.cs)|Dummy Implementation of ``ILocalizationProvider``|
|[``ScarletMessageProxy``](.\ScarletMessageProxy.cs)|``IScarletMessageProxy`` implementation used by the ``ScarletMessenger`` to forward and log Messages given to it. Logging is present in debug builds only.|
|[``ScarletMessenger``](.\ScarletMessenger.cs)|A lightweight event aggregator/messenger for loosely coupled communication. (Modified version of [Source](https://github.com/grumpydev/TinyMessenger)).|
|[``ScarletWeakEventManager``](.\ScarletWeakEventManager.cs)|Wrapper for [System.Windows.WeakEventManager](https://docs.microsoft.com/en-us/dotnet/api/system.windows.weakeventmanager) to support Dependency Inection.|

## Credits

``ScarletMessenger`` is largely based on [TinyMessenger](https://github.com/grumpydev/TinyMessenger), with a few tweaks here and there.

[back](../readme.md)
