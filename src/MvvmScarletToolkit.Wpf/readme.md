# MvvmScarletToolkit.Implementations

## Goals

This library aims to provide implementations for wpf specific classes used by the framework.

## Contents

|class|summary|
|---|---|
|[``AsyncCommand``](./AsyncCommand.cs)|Static factory for creating AsyncCommand instances.|
|[``ScarletCommandBuilder``](./Implementations/ScarletCommandBuilder.cs)|Fluent builder for ConcurrentCommand instances.|
|[``ScarletCommandManager``](./Implementations/ScarletCommandManager.cs)|Wrapper for [System.Windows.Input.CommandManager](https://docs.microsoft.com/en-gb/dotnet/api/system.windows.input.commandmanager) to support Dependency Injection.|
|[``ScarletDispatcher``](./Implementations/ScarletDispatcher.cs)|Wrapper for [System.Windows.Threading.Dispatcher](https://docs.microsoft.com/en-us/dotnet/api/system.windows.threading.dispatcher) to support Dependency Injection and Task based ICommand implementations in this library.|
|[``ScarletExitService``](./Implementations/ScarletExitService.cs)|In WPF ui controls dont always raise their Unloaded-event, when the application is being closed. ScarletExitService takes care of executing ``IVirtualizationViewModel.UnloadCommand`` when the application exits.|
|[``ScarletLocalizationProvider``](./Implementations/ScarletLocalizationProvider.cs)|Dummy Implementation of ``ILocalizationProvider``|
|[``ScarletWeakEventManager``](./Implementations/ScarletWeakEventManager.cs)|Wrapper for [System.Windows.WeakEventManager](https://docs.microsoft.com/en-us/dotnet/api/system.windows.weakeventmanager) to support Dependency Inection.|
