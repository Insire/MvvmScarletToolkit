# MvvmScarletToolkit

[![GitHub](https://img.shields.io/github/license/mashape/apistatus.svg)](https://github.com/Insire/Maple/blob/master/license.md)
![NuGet](https://img.shields.io/nuget/v/:MvvmScarletToolkit.svg)
[![Build status](https://dev.azure.com/SoftThorn/MvvmScarletToolkit/_apis/build/status/MvvmScarletToolkit-CI)](https://dev.azure.com/SoftThorn/MvvmScarletToolkit/_build/latest?definitionId=1)

MvvmScarletToolkit provides classes to speedup the MVVM development process primarily for WPF applications

|||||||||
|---|---|---|---|---|---|---|---|
|MvvmScarletToolkit|![.NET core 3.0](https://img.shields.io/badge/.NET-core3-blue.svg)|![.NET Framework 4.8](https://img.shields.io/badge/.NET-4.8-brightgreen.svg)|![.NET Framework 4.7.2](https://img.shields.io/badge/.NET-4.7.2-brightgreen.svg)|![.NET Framework 4.7.1](https://img.shields.io/badge/.NET-4.7.1-brightgreen.svg)|![.NET Framework 4.7](https://img.shields.io/badge/.NET-4.7-green.svg)|![.NET Framework 4.6.2](https://img.shields.io/badge/.NET-4.6.2-yellow.svg)|![.NET Framework 4.6.1](https://img.shields.io/badge/.NET-4.6.1-lightgrey.svg)|
|MvvmScarletToolkit.Commands|![.NET core 3.0](https://img.shields.io/badge/.NET-core3-blue.svg)|![.NET Framework 4.8](https://img.shields.io/badge/.NET-4.8-brightgreen.svg)|![.NET Framework 4.7.2](https://img.shields.io/badge/.NET-4.7.2-brightgreen.svg)|![.NET Framework 4.7.1](https://img.shields.io/badge/.NET-4.7.1-brightgreen.svg)|![.NET Framework 4.7](https://img.shields.io/badge/.NET-4.7-green.svg)|![.NET Framework 4.6.2](https://img.shields.io/badge/.NET-4.6.2-yellow.svg)|![.NET Framework 4.6.1](https://img.shields.io/badge/.NET-4.6.1-lightgrey.svg)|
|MvvmScarletToolkit.Observables|![.NET core 3.0](https://img.shields.io/badge/.NET-core3-blue.svg)|![.NET Framework 4.8](https://img.shields.io/badge/.NET-4.8-brightgreen.svg)|![.NET Framework 4.7.2](https://img.shields.io/badge/.NET-4.7.2-brightgreen.svg)|![.NET Framework 4.7.1](https://img.shields.io/badge/.NET-4.7.1-brightgreen.svg)|![.NET Framework 4.7](https://img.shields.io/badge/.NET-4.7-green.svg)|![.NET Framework 4.6.2](https://img.shields.io/badge/.NET-4.6.2-yellow.svg)|![.NET Framework 4.6.1](https://img.shields.io/badge/.NET-4.6.1-lightgrey.svg)|

## Features

- [MvvmScarletToolkit.Observables](MvvmScarletToolkit.Observables/readme.md)

  - INotifyPropertyChanged base classes for single instance viewmodels and collection viewmodels
  - Navigation ViewModels
  - Pagingviewmodel
  - Localization ViewModels (switching language without app restart)
  - Threadsafe busy state notifications via IBusyStack interface
  - EventAggregator

- [MvvmScarletToolkit.Commands](MvvmScarletToolkit.Commands/readme.md)

  - fluent interface to build task based icommands
    - cancellation support
    - limiting command execution to one per instance
    - state management and notification
  - basic synchronous icommand implementation

- [MvvmScarletToolkit](MvvmScarletToolkit/readme.md)

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
    - ConfigureableWindow (that opens where it was last closed)
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

## Samples

The included DemoApp contains examples for:

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

### TODO

- improve performance of the snake game and provide it as nuget package
- finalize basic FileSystemBrowser
- improve documentation
- improve readme with screenshots of properly working features
- add a nice icon for this lib
- add Contribution guidelines

Pre release nuget packages are available [here](https://pkgs.dev.azure.com/SoftThorn/_packaging/SoftThorn/nuget/v3/index.json).
I'll make them available to everyone when changes to the API get less common.
