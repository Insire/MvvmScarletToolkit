# MvvmScarletToolkit

[![License: MIT](https://img.shields.io/badge/License-MIT-blue.svg)](https://github.com/Insire/MvvmScarletToolkit/blob/master/LICENSE.md)
[![NuGet](https://img.shields.io/nuget/v/MvvmScarletToolkit.svg?style=flat-square&label=nuget)](https://www.nuget.org/packages/MvvmScarletToolkit/)
[![Build status](https://dev.azure.com/SoftThorn/MvvmScarletToolkit/_apis/build/status/MvvmScarletToolkit-CD)](https://dev.azure.com/SoftThorn/MvvmScarletToolkit/_build/latest?definitionId=1)
[![CodeFactor](https://www.codefactor.io/repository/github/insire/mvvmscarlettoolkit/badge)](https://www.codefactor.io/repository/github/insire/mvvmscarlettoolkit)
[![codecov](https://codecov.io/gh/Insire/MvvmScarletToolkit/branch/master/graph/badge.svg)](https://codecov.io/gh/Insire/MvvmScarletToolkit)

MvvmScarletToolkit provides classes to speed up the MVVM development process for XAML-based applications using the viewmodel first approach. It supports WPF and Avalonia.

## Features

Each package has its own readme with the full list of what it contains.

- [MvvmScarletToolkit.Commands](src/MvvmScarletToolkit.Commands/readme.md): fluent interface to build task based ICommands with cancellation, single execution and busy state notifications
- [MvvmScarletToolkit.Observables](src/MvvmScarletToolkit.Observables/readme.md): viewmodel base classes, threadsafe busy state, localization, change tracking, paging and navigation
- [MvvmScarletToolkit.Wpf](src/MvvmScarletToolkit.Wpf/readme.md): behaviors, converters as markup extensions, FileSystemBrowser, virtualizing panels, HtmlTextBlock and toast notifications
- [MvvmScarletToolkit.Avalonia](src/MvvmScarletToolkit.Avalonia/readme.md): Avalonia implementations of the toolkit services and async image loading
- [MvvmScarletToolkit.ImageLoading](src/MvvmScarletToolkit.ImageLoading/readme.md): loading and caching images from remote resources
- [MvvmScarletToolkit.Mediator](src/MvvmScarletToolkit.Mediator/readme.md): simple mediator with requests and notifications
- [MvvmScarletToolkit](src/MvvmScarletToolkit/readme.md): extension methods used across the toolkit
- [MvvmScarletToolkit.Abstractions](src/MvvmScarletToolkit.Abstractions/readme.md): interfaces shared by the rest of the toolkit

## Getting Started

### Install

Install the package for your UI framework. It brings in the commands, viewmodel base classes and abstractions.

For WPF:

```bash
dotnet add package MvvmScarletToolkit.Wpf
```

For Avalonia:

```bash
dotnet add package MvvmScarletToolkit.Avalonia
```

The current NuGet release (5.0.96) targets .NET 8 and 9, and is built against Avalonia 11. The source in this repository targets .NET 8 and 10, and Avalonia 12.

Pre-release packages are available on [GitHub](https://github.com/Insire/MvvmScarletToolkit/packages). GitHub requires a personal access token to restore from its NuGet feed.

### Create a viewmodel

Derive from `ViewModelBase` and create commands through the `IScarletCommandBuilder` passed to the constructor:

```cs
using MvvmScarletToolkit;
using MvvmScarletToolkit.Observables;
using System.Windows.Input;

public sealed class CounterViewModel : ViewModelBase
{
    private int _count;
    public int Count
    {
        get { return _count; }
        private set { SetProperty(ref _count, value); }
    }

    public ICommand IncrementCommand { get; }

    public CounterViewModel(IScarletCommandBuilder commandBuilder)
        : base(commandBuilder)
    {
        IncrementCommand = commandBuilder
            .Create(Increment)
            .WithSingleExecution() // only one execution at a time
            .WithBusyNotification(BusyStack) // sets IsBusy while running
            .WithAsyncCancellation() // adds cancellation support
            .Build();
    }

    private async Task Increment(CancellationToken token)
    {
        await Task.Delay(500, token);
        await Dispatcher.Invoke(() => Count++);
    }
}
```

For viewmodels that load their content, derive from `BusinessViewModelBase` or `BusinessViewModelListBase<T>` instead. They provide `LoadCommand`, `RefreshCommand` and `UnloadCommand`, and unload when the application exits.

### Create the viewmodel at startup

`ScarletCommandBuilder.Default` provides the services for the current UI framework. Use it once the application has started, because it depends on the running application. It can also be registered in a DI container as a singleton.

For WPF, in `App.xaml.cs` (remove `StartupUri` from `App.xaml`):

```cs
protected override void OnStartup(StartupEventArgs e)
{
    base.OnStartup(e);

    var window = new MainWindow
    {
        DataContext = new CounterViewModel(ScarletCommandBuilder.Default),
    };

    window.Show();
}
```

For Avalonia, in `App.axaml.cs`:

```cs
public override void OnFrameworkInitializationCompleted()
{
    if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
    {
        desktop.MainWindow = new MainWindow
        {
            DataContext = new CounterViewModel(ScarletCommandBuilder.Default),
        };
    }

    base.OnFrameworkInitializationCompleted();
}
```

### Bind the view

```xml
<StackPanel>
    <TextBlock Text="{Binding Count}" />
    <Button Command="{Binding IncrementCommand}" Content="Increment" />
    <ProgressBar IsIndeterminate="{Binding IsBusy}" />
</StackPanel>
```

The toolkit's XAML types share a single namespace in both frameworks. The WPF package has the larger set of converters and behaviors:

```xml
xmlns:mvvm="http://SoftThorn.MvvmScarletToolkit.com/winfx/xaml/shared"
```

### Explore the samples

The [samples](#samples) show these features in working applications. Requires the [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0):

```bash
git clone https://github.com/Insire/MvvmScarletToolkit.git
```

```bash
cd MvvmScarletToolkit
```

Run the WPF samples (Windows only):

```bash
dotnet run --project samples/MvvmScarletToolkit.Wpf.Samples
```

Run the Avalonia samples:

```bash
dotnet run --project samples/MvvmScarletToolkit.Avalonia.Samples
```

## Samples

- MvvmScarletToolkit.Core.Samples: viewmodels shared by the WPF and Avalonia samples
- MvvmScarletToolkit.Incubator: experimental dialog viewmodels used by the WPF samples

- MvvmScarletToolkit.Wpf.Samples

  - navigation
  - async cancelable commands using ICommand
  - file/folder browser
  - image loading with drag and drop, and in a virtualized list
  - lazy loading and data virtualization
  - live sorting and grouping in bound collections
  - DataGrid extensions: column filters, drag and drop grouping, hiding columns, summaries, saved layouts and exporting
  - a DataGrid with a million rows and the most common data types
  - an implementation of IProgress (e.g. for a progressbar) to send any amount of updates without locking up the UI thread
  - stress test for rendering lots of characters converted to geometry objects
  - a passwordbox with binding support
  - watermark support and a clear button for textboxes
  - dialogs and toast notifications
  - terminal/console and context menus using MVVM
  - nested selection

- MvvmScarletToolkit.Avalonia.Samples

  - navigation
  - async cancelable commands using ICommand
  - lazy loading and data virtualization
  - progress notifications
  - a passwordbox with binding support
  - terminal/console and context menus using MVVM

## Build Requirements

This repository requires the .NET 10 SDK, as configured in [global.json](./global.json). The easiest way to get everything is [Visual Studio 2026](https://visualstudio.microsoft.com/vs/community/) or newer, which prompts you to install any missing components when you open the [slnx](./MvvmScarletToolkit.slnx) file.

Otherwise, you need:

- Windows 10 or newer, or Linux (building only: the WPF projects and their tests and samples only run on Windows)
- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- [.NET 8 Runtime](https://dotnet.microsoft.com/download/dotnet/8.0) to run the tests for that target
- [Visual Studio Code](https://code.visualstudio.com/) with the [C# Dev Kit](https://marketplace.visualstudio.com/items?itemName=ms-dotnettools.csdevkit) provided by Microsoft
- [git](https://git-scm.com/)

Build the solution:

```bash
dotnet build MvvmScarletToolkit.slnx
```

Run the tests:

```bash
dotnet test --solution MvvmScarletToolkit.slnx
```

## AI Disclaimer

Parts of this codebase were written with the help of Claude, an AI assistant by Anthropic.

## TODO

- finalize basic FileSystemBrowser
- add more tests
- improve documentation
- add a nice icon for this lib
- add Contribution guidelines
