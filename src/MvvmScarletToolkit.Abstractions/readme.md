# MvvmScarletToolkit.Abstractions

![.NET standard 2.0](https://img.shields.io/badge/.NET-standard2.0-brightgreen)

## Goals

This library aims to provide interfaces and extensions that are being used by the rest of the framework.

## Features

The interfaces in this package keep viewmodels independent of a UI framework. Reference it directly when writing your own implementations, or a library that should not depend on WPF or Avalonia.

### Platform services

|interface|summary|
|---|---|
|``IScarletDispatcher``|Run code on the UI thread.|
|``IScarletCommandManager``|Notify commands that their ``CanExecute`` state may have changed.|
|``IScarletEventManager<TEventSource, TEventArgs>``|Subscribe to events through weak references.|
|``IExitService``|Unload viewmodels when the application exits, and wait for that to complete with ``ShutDown``.|

### Commands

|interface|summary|
|---|---|
|``IConcurrentCommand``|``ICommand`` that exposes its running ``Task`` and state.|
|``IAsyncCommand``|Disposable ``IConcurrentCommand``.|
|``ICancelCommand``|Command that cancels a running ``IConcurrentCommand``.|
|``IScarletExceptionHandler``|Receives exceptions thrown by commands.|

### State

|interface|summary|
|---|---|
|``IBusyStack``|Tracks running operations through disposable tokens.|
|``IObservableBusyStack``|``IBusyStack`` that can also be observed.|
|``IChangeTracker``, ``IToolkitChangeTracker``|Track viewmodels for changes.|
|``IVirtualizationViewModel``|Viewmodel with a load, refresh and unload lifecycle.|
|``IBusinessViewModelListBase<TViewModel>``|Collection version of ``IVirtualizationViewModel``.|

### Localization

|interface|summary|
|---|---|
|``ILocalizationProvider``|Translates keys for a culture and lists the available languages. Implement this to connect your own translations.|
|``ILocalizationService``|Translates keys for the current language.|
|``ILocalizationViewModel``|Binding support for a single localized string.|

### Image loading

These types are used by [MvvmScarletToolkit.ImageLoading](../MvvmScarletToolkit.ImageLoading/readme.md).

|type|summary|
|---|---|
|``IImageService<T>``|Provides images for a uri and size.|
|``IImageFactory<TImage>``|Turns a stream into an image of the UI framework.|
|``IImageDataProvider``|Fetches the content of a uri.|
|``IImageDataCache``, ``IImageDataMemoryCache``, ``IImageDataFileystemCache``|Caches for raw image data.|
|``IImageMemoryCache<TImage>``, ``IImageFilesystemCache<TImage>``|Caches for decoded images.|
|``ImageSize``|Width and height of an image.|
|``ImageLoadState``|State of an image that gets loaded asynchronously: ``None``, ``Pending``, ``Loading``, ``Loaded`` or ``Failed``.|

### DataGrid export

These types let applications export a grid without the toolkit depending on a format. See [Exporting a DataGrid](../../docs/datagrid-export.md).

|type|summary|
|---|---|
|``IDataGridExporter``|Turns the content of a grid into something else, like a file.|
|``IDataGridDataSource``|Snapshot of a grid: its visible columns in display order, and its rows after filtering in the order shown on screen.|
|``DataGridItems<TItem>``|The rows of a grid, and a description of its visible columns.|
|``DataGridColumnInfo``|Name and caption of a column.|

### Builders

|type|summary|
|---|---|
|``IBuilder<TElement>``, ``AbstractBuilder<TElement>``|Base for fluent builders, e.g. the command builder.|
