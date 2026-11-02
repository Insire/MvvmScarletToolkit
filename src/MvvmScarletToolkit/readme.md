# MvvmScarletToolkit

![.NET standard 2.0](https://img.shields.io/badge/.NET-standard2.0-brightgreen)

## Goals

This library aims to provide concrete classes and implementations for the rest of the framework.

## Features

### Collections

|method|summary|
|---|---|
|``ForEach``|Run an action for each item, optionally stopping when a ``CancellationToken`` is cancelled. Returns the source for further chaining.|
|``ForEachAsync``|Run an asynchronous function for each item, optionally limited to a maximum number of parallel executions.|
|``Batch``|Split a sequence into batches of a given size.|
|``DistinctBy``|Remove duplicates by a key.|
|``AddRange``|Add several items to an ``ICollection<T>``.|
|``Dispose``|Dispose every item of a sequence of ``IDisposable``.|
|``UpdateItems``|Update a list from another sequence. Items matched by a comparer are updated, new ones are added and missing ones are removed.|
|``Sort``|Sort an ``ObservableCollection<T>`` in place, optionally with an ``IComparer<T>``.|

### Queries

|method|summary|
|---|---|
|``TrySkip``, ``TryTake``|Apply ``Skip`` or ``Take`` to an ``IQueryable<T>`` when a value is provided.|
|``TryPage``|Apply both for paging.|

### Other

|method or class|summary|
|---|---|
|``CommandExtensions.TryExecute``|Execute an ``ICommand``, if ``CanExecute`` allows it.|
|``DispatcherExtensions.Invoke``|Run an action or function through an ``IScarletDispatcher``, without passing a ``CancellationToken``.|
|``EnumExtensions.GetAttributeOfType``|Read an attribute from an enum value, e.g. a ``DescriptionAttribute``.|
|``EventExtension.Raise``|Raise an ``EventHandler``, if it has subscribers.|
|``TypeExtensions.GetGenericTypeName``|Get a readable name for a generic type, like ``List<Int32>``.|
|``CancellableGenericScarletMessage<T>``|Message for the ``IMessenger``, that carries content and a ``Cancel`` action for the receiver.|

## Usage

```cs
using MvvmScarletToolkit;

await paths.ForEachAsync(path => Upload(path, token), maxDoP: 4, token);

foreach (var batch in items.Batch(100))
{
    await Save(batch);
}

await Dispatcher.Invoke(() => Count++);

var page = query.TryPage(index, count);
```
