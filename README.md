# MvvmScarletToolkit

[![License: MIT](https://img.shields.io/badge/License-MIT-blue.svg)](https://github.com/Insire/Maple/blob/master/license.md)
[![NuGet](https://img.shields.io/nuget/v/MvvmScarletToolkit.svg?style=flat-square&label=nuget)](https://www.nuget.org/packages/MvvmScarletToolkit/)
[![Build status](https://dev.azure.com/SoftThorn/MvvmScarletToolkit/_apis/build/status/MvvmScarletToolkit-CD)](https://dev.azure.com/SoftThorn/MvvmScarletToolkit/_build/latest?definitionId=1)
[![CodeFactor](https://www.codefactor.io/repository/github/insire/mvvmscarlettoolkit/badge)](https://www.codefactor.io/repository/github/insire/mvvmscarlettoolkit)
[![codecov](https://codecov.io/gh/Insire/MvvmScarletToolkit/branch/master/graph/badge.svg)](https://codecov.io/gh/Insire/MvvmScarletToolkit)

MvvmScarletToolkit provides classes to speedup the MVVM development process for xaml-based applications using the viewmodel first approach.

Pre release nuget packages are available [here](https://pkgs.dev.azure.com/SoftThorn/_packaging/SoftThorn/nuget/v3/index.json) and on [github](https://github.com/Insire/MvvmScarletToolkit/packages).

## Features

- [MvvmScarletToolkit.Commands](src/MvvmScarletToolkit.Commands/readme.md)

  - fluent interface to build task based icommands
    - cancellation support
    - limiting command execution to one per instance at any given time
    - state management and notification

- [MvvmScarletToolkit.Observables](src/MvvmScarletToolkit.Observables/readme.md)

  - INotifyPropertyChanged base classes for single instance viewmodels and collection viewmodels
  - Localization ViewModels (switching language without app restart)
  - Threadsafe busy state notifications via IBusyStack interface
  - Helper viewmodels for
    - paging
    - enums
    - binding to structs and value types
    - basic versioning/change tracking
    - navigation

- [MvvmScarletToolkit.Wpf](src/MvvmScarletToolkit.Wpf/readme.md)

  - BindingProxy
  - Features
    - Support for * Sizing in GridViews
    - Support for providing an explicit list of datatemplates for a datatemplate selector
    - Support for managing groupings from an ICollectionView
    - Support for loading a System.Drawing.Bitmap into a BitmapSource without copying its data
  - Attached Properties
    - Filter
      - binding ICollectionView.Filter
      - refresh ICollectionView on property change
    - Focus
      - set focus on window load
  - TriggerActions
    - ClearPasswordBoxAction
    - ClearTextBoxAction
  - Behaviors
    - MultiSelectionBehavior: bind MultiSelector.SelectedItems (for DataGrid)
    - MultiListBoxSelectionBehavior: bind ListBx.SelectedItems
    - SelectedTreeViewItemBehavior: bind SelectedItem for TreeView
    - PasswordBindingBehavior: bind PasswordBox.Password
    - WatermarkBehavior for TextBoxes
    - AutoRepositionPopupBehavior
    - LaunchNavigateUriAsNewProcessBehavior: Launch Hyperlink.NavigateUri in a new Process
    - ScrollSelectionIntoViewBehavior: scroll SelectedItem into View for DataGrid and ListBox
    - ScrollToEndCommandBehavior: execute a command, when a scrollviewer has been scrolled to its vertical or horizontal end
    - AutoScrollBehavior: auto scrolling for Selector (e.g. ListBox and DataGrid)
  - Controls
    - FileSystemBrowser
    - VirtualizingTilePanel
    - VirtualizingWrapPanel
  - (Multi-)Converters as Markupextension
    - ToCase (convert a string to a given casing)
    - SmallerThan (whether a bound number is smaller than the converter value)
    - GreaterThan (whether a bound number is larger than the converter value)
    - IsNot (negate a boolean)
    - IsNotNull (return whether something is not null)
    - IsNotNullOrEmpty (return whether a string is not null or empty)
    - IsNotNullOrWhiteSpace (return whether a string is not null or whitespace)
    - IsNull (return whether something is null)
    - IsNullOrEmpty (return whether a string is null or empty)
    - IsNullOrWhiteSpace (return whether a string is null or whitespace)
    - DebugConverter (log bound values and value changes to the Debug.Listener)
    - InvertBooleanToVisibilityConverter (negate a boolean to visibility)
    - RadioButtonCheckedConverter (compares the bound value to the argument and returns whether they are equal)
    - MultiBooleanAndConverter (combines all conditions with logical and)
  - MarkupExtensions
    - ConverterMarkupExtension
    - MultiConverterMarkupExtension
    - EnumBindingSourceExtension (create an itemsource from an enum type)
    - StartProcessExtension (let the OS open an uri)
  - Extensions
    - DependencyObjectExtensions

- [MvvmScarletToolkit.Xamarin.Forms](src/MvvmScarletToolkit.Xamarin.Forms/readme.md)

  - BindingProxy
  - Converters:
    - IsNot (negate a boolean)
    - IsNotNull (return whether something is not null)
    - IsNotNullOrEmpty (return whether a string is not null or empty)
    - IsNotNullOrWhiteSpace (return whether a string is not null or whitespace)
    - IsNull (return whether something is null)
    - IsNullOrEmpty (return whether a string is null or empty)
    - IsNullOrWhiteSpace (return whether a string is null or whitespace)
  - MarkupExtension
    - ColorToHexExtension
  - Behaviors
    - BehaviorBase (provides the currently associated control that also forwwards updates to the bindingcontext as it changes on the control)
    - EventToCommandBehavior

- [MvvmScarletToolkit.Abstractions](src/MvvmScarletToolkit.Abstractions/readme.md)

  - EnumerableExtensions
  - ListExtensions
  - EnumExtensions
  - TypeExtensions
  - EventExtensions

- MvvmScarletToolkit.Wpf.Samples (for WPF)

  - simple navigation
  - master detail view
  - async cancelable commands using ICommand
  - simple file/folder browser
  - barebones drag and drop example
  - an implementation of IProgress (e.g for a progressbar) to send any amount of updates without locking   up the UI thread
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
- [Net 5.0](https://dotnet.microsoft.com/download/dotnet-core/5.0)
- [Net 6.0](https://dotnet.microsoft.com/download/dotnet-core/6.0)
- [Visual Studio Code](https://code.visualstudio.com/) with the [C# Extension](https://github.com/OmniSharp/omnisharp-vscode) provided by Microsoft
- [git](https://git-scm.com/)

(This should be everything, but it's possible i missed something. So please tell me if that's the case.)

### TODO

- finalize basic FileSystemBrowser
- add more tests
- improve documentation
- add a nice icon for this lib
- add Contribution guidelines
