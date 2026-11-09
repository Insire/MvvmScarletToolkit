<h1>
  <picture>
    <source media="(prefers-color-scheme: dark)" srcset="assets/branding/wordmark-dark.svg">
    <img src="assets/branding/wordmark.svg" alt="MvvmScarletToolkit" width="420">
  </picture>
</h1>

[![License: MIT](https://img.shields.io/badge/License-MIT-blue.svg)](https://github.com/Insire/MvvmScarletToolkit/blob/master/LICENSE.md)
[![NuGet](https://img.shields.io/nuget/v/MvvmScarletToolkit.svg?style=flat-square&label=nuget)](https://www.nuget.org/packages/MvvmScarletToolkit/)
[![Build status](https://dev.azure.com/SoftThorn/MvvmScarletToolkit/_apis/build/status/CD?branchName=master)](https://dev.azure.com/SoftThorn/MvvmScarletToolkit/_build/latest?definitionId=10&branchName=master)
[![CodeFactor](https://www.codefactor.io/repository/github/insire/mvvmscarlettoolkit/badge)](https://www.codefactor.io/repository/github/insire/mvvmscarlettoolkit)
[![codecov](https://codecov.io/gh/Insire/MvvmScarletToolkit/branch/master/graph/badge.svg)](https://codecov.io/gh/Insire/MvvmScarletToolkit)

MvvmScarletToolkit provides classes to speed up the MVVM development process for XAML-based applications using the viewmodel first approach. It supports WPF and Avalonia.

## Features

Each package has its own readme with the full list of what it contains.

- [MvvmScarletToolkit.Commands](src/MvvmScarletToolkit.Commands/readme.md): fluent interface to build task based ICommands with cancellation, single execution and busy state notifications
- [MvvmScarletToolkit.Observables](src/MvvmScarletToolkit.Observables/readme.md): viewmodel base classes, threadsafe busy state, localization, change tracking, paging and navigation
- [MvvmScarletToolkit.Wpf](src/MvvmScarletToolkit.Wpf/readme.md): behaviors, converters as markup extensions, DataGrid extensions, FileSystemBrowser with file and folder dialogs, virtualizing panels, HtmlTextBlock, selectable text and toast notifications
- [MvvmScarletToolkit.Avalonia](src/MvvmScarletToolkit.Avalonia/readme.md): Avalonia implementations of the toolkit services, async image loading, virtualizing panels and HtmlTextBlock
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

All packages except MvvmScarletToolkit.Wpf are trim and Native AOT compatible. The package readmes list the few APIs that need extra care. The Avalonia sample publishes as a native executable with `dotnet publish samples/MvvmScarletToolkit.Avalonia.Samples -c Release -r win-x64` (or `linux-x64`). See [Native AOT](#native-aot) for the required native toolchain.

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
- MvvmScarletToolkit.Incubator: experimental WPF image code referenced by the WPF samples

- MvvmScarletToolkit.Wpf.Samples

  - navigation
  - async cancelable commands using ICommand
  - file/folder browser
  - file and folder dialogs with file filters
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
  - binding enum values and an ObservableDictionary
  - a tag cloud with a VirtualizingWrapPanel
  - selecting and copying the text of a TextBlock

- MvvmScarletToolkit.Avalonia.Samples: the same scenes as the WPF samples, apart from the DataGrid, grouping, the file/folder browser and text selection, as those are not part of the Avalonia package

  - navigation
  - async cancelable commands using ICommand
  - image loading with drag and drop, and in a virtualized list
  - lazy loading and data virtualization
  - progress notifications and state changes in a tree structure
  - stress test for rendering lots of characters converted to geometry objects, in a VirtualizingTilePanel
  - a passwordbox with binding support
  - input prevention for textboxes
  - dialogs and toast notifications
  - terminal/console and context menus using MVVM
  - nested selection
  - binding enum values and an ObservableDictionary
  - a tag cloud with a VirtualizingWrapPanel

## Building

### Requirements

- Windows 10 or newer, or Linux. On Linux the WPF projects build, but their tests and samples only run on Windows.
- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0), as configured in [global.json](./global.json)
- [.NET 8 Runtime](https://dotnet.microsoft.com/download/dotnet/8.0), only to run the tests that target .NET 8

On Windows, both install with:

```bash
winget install Microsoft.DotNet.SDK.10 Microsoft.DotNet.Runtime.8
```

On Linux, install them with your package manager or the [install script](https://learn.microsoft.com/dotnet/core/install/linux-scripted-manual).

### Build and test

Build the solution:

```bash
dotnet build MvvmScarletToolkit.slnx
```

Run the tests:

```bash
dotnet test --solution MvvmScarletToolkit.slnx
```

### Build like the CI

`build.ps1` (Windows) and `build.sh` (Linux) run the [Cake](./cake) build used by the pipelines. It formats the XAML files, runs the tests with code coverage and packs the NuGet packages into `packages`. Reports end up in `results`.

```bash
./build.ps1
```

The CD pipeline publishes a GitHub release for every build of `master`, named and tagged with the computed version (e.g. `5.0.96`). Its notes come from the Cake target `CreateReleaseNotes`, which lists the commits since the previous release tag. Preview them in `results/release-notes.md` with:

```bash
./build.ps1 --target=CreateReleaseNotes
```

### Native AOT

Only needed to publish the Avalonia sample or the AOT tests as a native executable, not to build the repository. It needs a platform C toolchain:

- Windows: [Visual Studio 2026](https://visualstudio.microsoft.com/vs/community/) or the Build Tools with the **Desktop development with C++** workload
- Linux: a C compiler and linker (`clang` or `gcc`) and the zlib headers, e.g. `sudo apt install clang zlib1g-dev` (Debian, Ubuntu) or `sudo dnf install clang zlib-devel` (Fedora)

```bash
dotnet publish samples/MvvmScarletToolkit.Avalonia.Samples -c Release -r win-x64
```

Use `-r linux-x64` on Linux.

`MvvmScarletToolkit.Aot.Tests` runs the tests of the AOT compatible packages as a native executable. Publish it, then start it from its publish folder:

```bash
dotnet publish tests/MvvmScarletToolkit.Aot.Tests -c Release -r win-x64
```

## AI Disclaimer

Parts of this codebase were written with the help of Claude, an AI assistant by Anthropic.

## TODO

- add Contribution guidelines
