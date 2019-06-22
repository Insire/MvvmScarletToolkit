# MvvmScarletToolkit

[![GitHub](https://img.shields.io/github/license/mashape/apistatus.svg)](https://github.com/Insire/Maple/blob/master/license.md)
![NuGet](https://img.shields.io/nuget/v/:MvvmScarletToolkit.svg)
[![Build status](https://ci.appveyor.com/api/projects/status/cr38mi88wes4shj7/branch/master?svg=true)](https://ci.appveyor.com/project/Insire/mvvmscarlettoolkit/branch/master)

MvvmScarletToolkit provides classes to speedup the MVVM development process primarily for WPF applications

||||||||
|---|---|---|---|---|---|---|
|MvvmScarletToolkit|![.NET core 3.0](https://img.shields.io/badge/.NET-core3-blue.svg)|![.NET Framework 4.7.1](https://img.shields.io/badge/.NET-4.7.1-brightgreen.svg)|![.NET Framework 4.7](https://img.shields.io/badge/.NET-4.7-green.svg)|![.NET Framework 4.6.2](https://img.shields.io/badge/.NET-4.6.2-yellow.svg)|![.NET Framework 4.6.1](https://img.shields.io/badge/.NET-4.6.1-lightgrey.svg)|![.NET Framework 4.6](https://img.shields.io/badge/.NET-4.6-lightgrey.svg)|
|MvvmScarletToolkit.Abstractions|![.NET core 3.0](https://img.shields.io/badge/.NET-core3-blue.svg)|![.NET Framework 4.7.1](https://img.shields.io/badge/.NET-4.7.1-brightgreen.svg)|![.NET Framework 4.7](https://img.shields.io/badge/.NET-4.7-green.svg)|![.NET Framework 4.6.2](https://img.shields.io/badge/.NET-4.6.2-yellow.svg)|![.NET Framework 4.6.1](https://img.shields.io/badge/.NET-4.6.1-lightgrey.svg)|![.NET Framework 4.6](https://img.shields.io/badge/.NET-4.6-lightgrey.svg)|
|MvvmScarletToolkit.Commands|![.NET core 3.0](https://img.shields.io/badge/.NET-core3-blue.svg)|![.NET Framework 4.7.1](https://img.shields.io/badge/.NET-4.7.1-brightgreen.svg)|![.NET Framework 4.7](https://img.shields.io/badge/.NET-4.7-green.svg)|![.NET Framework 4.6.2](https://img.shields.io/badge/.NET-4.6.2-yellow.svg)|![.NET Framework 4.6.1](https://img.shields.io/badge/.NET-4.6.1-lightgrey.svg)|![.NET Framework 4.6](https://img.shields.io/badge/.NET-4.6-lightgrey.svg)|
|MvvmScarletToolkit.ConfigurableWindow|![.NET core 3.0](https://img.shields.io/badge/.NET-core3-blue.svg)|![.NET Framework 4.7.1](https://img.shields.io/badge/.NET-4.7.1-brightgreen.svg)|![.NET Framework 4.7](https://img.shields.io/badge/.NET-4.7-green.svg)|![.NET Framework 4.6.2](https://img.shields.io/badge/.NET-4.6.2-yellow.svg)|![.NET Framework 4.6.1](https://img.shields.io/badge/.NET-4.6.1-lightgrey.svg)|![.NET Framework 4.6](https://img.shields.io/badge/.NET-4.6-lightgrey.svg)|
|MvvmScarletToolkit.FileSystemBrowser|![.NET core 3.0](https://img.shields.io/badge/.NET-core3-blue.svg)|![.NET Framework 4.7.1](https://img.shields.io/badge/.NET-4.7.1-brightgreen.svg)|![.NET Framework 4.7](https://img.shields.io/badge/.NET-4.7-green.svg)|![.NET Framework 4.6.2](https://img.shields.io/badge/.NET-4.6.2-yellow.svg)|![.NET Framework 4.6.1](https://img.shields.io/badge/.NET-4.6.1-lightgrey.svg)|![.NET Framework 4.6](https://img.shields.io/badge/.NET-4.6-lightgrey.svg)|
|MvvmScarletToolkit.Incubator|![.NET core 3.0](https://img.shields.io/badge/.NET-core3-blue.svg)|![.NET Framework 4.7.1](https://img.shields.io/badge/.NET-4.7.1-brightgreen.svg)|![.NET Framework 4.7](https://img.shields.io/badge/.NET-4.7-green.svg)|![.NET Framework 4.6.2](https://img.shields.io/badge/.NET-4.6.2-yellow.svg)|![.NET Framework 4.6.1](https://img.shields.io/badge/.NET-4.6.1-lightgrey.svg)|![.NET Framework 4.6](https://img.shields.io/badge/.NET-4.6-lightgrey.svg)|
|MvvmScarletToolkit.Observables|![.NET core 3.0](https://img.shields.io/badge/.NET-core3-blue.svg)|![.NET Framework 4.7.1](https://img.shields.io/badge/.NET-4.7.1-brightgreen.svg)|![.NET Framework 4.7](https://img.shields.io/badge/.NET-4.7-green.svg)|![.NET Framework 4.6.2](https://img.shields.io/badge/.NET-4.6.2-yellow.svg)|![.NET Framework 4.6.1](https://img.shields.io/badge/.NET-4.6.1-lightgrey.svg)|![.NET Framework 4.6](https://img.shields.io/badge/.NET-4.6-lightgrey.svg)|

## Features

- [MvvmScarletToolkit.Observables](.\MvvmScarletToolkit.Observables\readme.md)
    - INotifyPropertyChanged base classes for single instance viewmodels and collection viewmodels
    - Navigation ViewModels
    - Localization ViewModels
    - Threadsafe busy state notifications via IBusyStack interface
    - Changetracking
    - misc observable collections
- [MvvmScarletToolkit.Commands](.\MvvmScarletToolkit.Commands\readme.md)
    - fluent interface to build task based icommands
        - cancellation support
        - limiting command execution to one per instance
    - basic synchronous icommand
- [MvvmScarletToolkit](.\MvvmScarletToolkit\readme.md)
    - BindingProxy
    - VirtualizingTilePanel
    - NullValueConverter
    - EnumBinding support via markup extension
    - a few LinQ extensions

## Samples

The included DemoApp contains examples for:
- simple navigation
- master detail view
- async cancelable commands using ICommand
- async(background) image loading
- simple file/folder browser
- a snake game
- barebones drag and drop example

### TODO
- improve performance of the snake game and provide it as nuget package
- improve Async Commands
- finalize basic FileSystemBrowser
- improve documentation
- improve readme with screenshots of properly working features
- improve setup CI with appveyor
- add a nice icon for this lib
- add Contribution guidelines

