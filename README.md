# MvvmScarletToolkit

[![GitHub](https://img.shields.io/github/license/mashape/apistatus.svg)](https://github.com/Insire/Maple/blob/master/license.md)
![NuGet](https://img.shields.io/nuget/v/:MvvmScarletToolkit.svg)
[![Build status](https://dev.azure.com/SoftThorn/MvvmScarletToolkit/_apis/build/status/MvvmScarletToolkit-CD)](https://dev.azure.com/SoftThorn/MvvmScarletToolkit/_build/latest?definitionId=1)
[![CodeFactor](https://www.codefactor.io/repository/github/insire/mvvmscarlettoolkit/badge)](https://www.codefactor.io/repository/github/insire/mvvmscarlettoolkit)

MvvmScarletToolkit provides classes to speedup the MVVM development process for xaml-based applications using the viewmodel first approach.

## Supported .Net Framework versions

![.NET core 3.1](https://img.shields.io/badge/.NET-core31-blue.svg) ![.NET Framework 4.8](https://img.shields.io/badge/.NET-4.8-brightgreen.svg) ![.NET Framework 4.7.2](https://img.shields.io/badge/.NET-4.7.2-brightgreen.svg) ![.NET Framework 4.7.1](https://img.shields.io/badge/.NET-4.7.1-brightgreen.svg) ![.NET Framework 4.7](https://img.shields.io/badge/.NET-4.7-green.svg) ![.NET Framework 4.6.2](https://img.shields.io/badge/.NET-4.6.2-yellow.svg) ![.NET Framework 4.6.1](https://img.shields.io/badge/.NET-4.6.1-lightgrey.svg)

## Features

- [MvvmScarletToolkit.Messenger](src/MvvmScarletToolkit.Messenger/readme.md)

  - ScarletMessenger (Event Aggregator)

- [MvvmScarletToolkit.Commands](src/MvvmScarletToolkit.Commands/readme.md)

  - fluent interface to build task based icommands
    - cancellation support
    - limiting command execution to one per instance
    - state management and notification
  - basic synchronous icommand implementation

- [MvvmScarletToolkit.Observables](src/MvvmScarletToolkit.Observables/readme.md)

  - INotifyPropertyChanged base classes for single instance viewmodels and collection viewmodels
  - Navigation ViewModels
  - Paging Viewmodel
  - Localization ViewModels (switching language without app restart)
  - Threadsafe busy state notifications via IBusyStack interface

- [MvvmScarletToolkit.Wpf](src/MvvmScarletToolkit.Wpf/readme.md)

  - BindingProxy
  - EnumBinding support via markup extension
  - Behaviors
    - MultiSelectionBehavior for ListBoxes
    - SelectedTreeViewItemBehavior
    - PasswordBindingBehavior
    - WatermarkBehavior for TextBoxes
  - Controls
    - VirtualizingTilePanel
    - FileSystemBrowser
    - Support for * Sizing in GridViews
  - (Multi-)Converters as Markupextension
    - NullValueConverter
    - InvertBooleanConverter
    - RadioButtonCheckedConverter
    - CaseConverter
  - Extensions
    - DependencyObjectExtensions
    - EnumerableExtensions
    - ListExtensions

- [MvvmScarletToolkit.Xamarin.Forms](src/MvvmScarletToolkit.Xamarin.Forms/readme.md)

  - BindingProxy
  - (Multi-)Converters as Markupextension
    - InvertBooleanConverter
    - ColorToHexExtension

## Samples

The included Wpf Samples App contains examples for:

- simple navigation
- master detail view
- async cancelable commands using ICommand
- async(background) image loading
- simple file/folder browser
- a snake game
- barebones drag and drop example
- an implementation of IProgress (e.g for a progressbar) to send any amount of updates without locking up the UI thread
- stress test for rendering lots of charachters converted to geometry objects
- a passwordbox with binding support
- watermark support for textboxes (custom watermark style included)
- easy grouping for bound datagrids
- clear button for textboxes

## Build Requirements

This library uses SDK-style project files, which means you are required to use [Visual Studio 2019](https://visualstudio.microsoft.com/vs/community/) or newer. Visual Studio will prompt you to install any missing components once you open the [sln](./MvvmScarletToolkit.sln) file.

For anyone not wishing to install that, they atleast need:

- Windows 10 (older versions work probably too, but the repository is not configured for those)
- .net 4.6.1 (preinstalled starting with Windows 10)
- [Net Core 3.1](https://dotnet.microsoft.com/download/dotnet-core/3.1)
- [Visual Studio Code](https://code.visualstudio.com/) with the [C# Extension](https://github.com/OmniSharp/omnisharp-vscode) provided by Microsoft
- [git](https://git-scm.com/)

(This should be everything, but it's possible i missed something. So please tell me if that's the case.)

### TODO

- improve performance of the snake game and provide it as nuget package
- finalize basic FileSystemBrowser
- improve documentation
- improve readme with screenshots of properly working features
- add a nice icon for this lib
- add Contribution guidelines

Pre release nuget packages are available [here](https://pkgs.dev.azure.com/SoftThorn/_packaging/SoftThorn/nuget/v3/index.json).
I'll make them available to everyone when changes to the API get less common.
