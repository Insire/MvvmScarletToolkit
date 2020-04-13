# MvvmScarletToolkit.Implementations

## Goals

This library aims to provide implementations for wpf specific classes used by the framework.

## Contents

|class|summary|
|---|---|
|[``ScarletCommandBuilder``](.\ScarletCommandBuilder.cs)|Fluent builder for ConcurrentCommand instances.|
|[``ScarletCommandManager``](.\ScarletCommandManager.cs)|Wrapper for [System.Windows.Input.CommandManager](https://docs.microsoft.com/en-gb/dotnet/api/system.windows.input.commandmanager) to support Dependency Injection.|
|[``ScarletDispatcher``](.\ScarletDispatcher.cs)|Wrapper for [System.Windows.Threading.Dispatcher](https://docs.microsoft.com/en-us/dotnet/api/system.windows.threading.dispatcher) to support Dependency Injection and Task based ICommand implementations in this library.|
|[``ScarletExitService``](.\ScarletExitService.cs)|Currently does nothing, until i can figure out how xamarin forms does handle app shutdowns - if at all|
|[``ScarletWeakEventManager``](.\ScarletWeakEventManager.cs)|Wrapper for WeakEventManager. ([See](https://github.com/xamarin/Xamarin.Forms/issues/8405))|
