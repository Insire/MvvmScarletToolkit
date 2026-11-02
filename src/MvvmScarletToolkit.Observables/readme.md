# MvvmScarletToolkit.Observables

![.NET standard 2.0](https://img.shields.io/badge/.NET-standard2.0-brightgreen)

## Goals

This library aims to provide base and utility classes around state and state management in viewmodels in the MVVM pattern.

## Features

- **Viewmodel base classes:** single instance and collection viewmodels built on [CommunityToolkit.Mvvm](https://github.com/CommunityToolkit/dotnet). They provide the dispatcher, messenger, command builder and a busy state.
- **Load, refresh and unload lifecycle:** business viewmodels provide ready to bind commands, track `IsLoaded` and unload themselves when the application exits.
- **Threadsafe collection changes:** `ViewModelListBase<T>` updates its items on the UI thread, so they can be changed from any thread.
- **Selection:** collection viewmodels track `SelectedItem` and `SelectedItems`, and provide commands to remove or clear items.
- **Busy state:** `BusyStack` counts running operations through disposable tokens and reports whether any are left. `ObservableBusyStack` also lets other classes subscribe, which allows forwarding the state between viewmodels.
- **Localization:** switch the language without restarting the application. Bound strings update on their own.
- **Change tracking:** detect changed viewmodels through `INotifyPropertyChanged`, or through `PropertyChangedMessage<T>` to also get the old and new values.
- **Paging:** page through data with commands for the next, previous, first and last page, and a selectable page size.
- **DynamicData support:** keyed viewmodel lists based on [DynamicData](https://github.com/reactivemarbles/DynamicData), for filtering and sorting large or frequently changing sets.
- **Observable collections:** `ObservableDictionary<TKey, TValue>` and the fixed size `ObservableCircularBuffer<T>`.
- **Throttled progress:** `DispatcherProgress<T>` limits how often progress updates reach the UI thread, so frequent reports don't block it.
- **Helper viewmodels:** navigation, enums, binding to structs and value types, and tracking a value against its default.

## Contents

### ViewModels

|class|summary|
|---|---|
|[``ViewModelBase``](ViewModels/Base/ViewModelBase.cs)|Base viewmodel that serves as service aggregate, and provides ``IsBusy`` and ``BusyStack``.|
|[``ViewModelBase<T>``](ViewModels/Base/ViewModelBase_generic.cs)|Generic version of ``ViewModelBase`` exposing an injected (model)class through ``Model``.|
|[``ViewModelListBase<T>``](ViewModels/Base/ViewModelListBase.cs)|Base class for collections, with threadsafe methods to change its items, selection support, and ``RemoveCommand``, ``RemoveRangeCommand`` and ``ClearCommand``.|
|[``BusinessViewModelBase``](ViewModels/Base/BusinessViewModelBase.cs)|``ViewModelBase`` that bootstraps loading, unloading and refreshing of its content.|
|[``BusinessViewModelBase<T>``](ViewModels/Base/BusinessViewModelBase_generic.cs)|Generic version of ``BusinessViewModelBase`` exposing a model.|
|[``BusinessViewModelListBase<T>``](ViewModels/Base/BusinessViewModelListBase.cs)|``ViewModelListBase<T>`` that bootstraps loading, unloading and refreshing of its content.|
|[``DomainViewModelListBase<T>``](ViewModels/Base/DomainViewModelListBase.cs)|``BusinessViewModelListBase<T>`` that adds paged loading through ``PageIndex``, ``PageSize``, ``Total`` and ``Paging``.|
|[``PagingViewModel<T>``](ViewModels/PagingViewModel.cs)|Commands for navigating the pages of a ``DomainViewModelListBase<T>``.|
|[``ViewModelContainer<T>``](ViewModels/ViewModelContainer.cs)|Generic wrapper viewmodel to add binding support to any object, including structs and value types.|
|[``EnumViewModel<T>``](ViewModels/EnumViewModel_generic.cs)|``ViewModelContainer<T>`` for an enum value with a display name.|
|[``VersionViewModel<T>``](ViewModels/VersionViewModel.cs)|Tracks a value against its default and exposes ``CurrentOrDefault``.|

### DynamicData

|class|summary|
|---|---|
|[``SourceListViewModelBase<T>``](ViewModels/Base/Rx/SourceListViewModelBase.cs)|Keyed viewmodel collection, that exposes its changes through ``Connect``.|
|[``BusinessSourceListViewModelBase<T>``](ViewModels/Base/Rx/BusinessSourceListViewModelBase.cs)|``SourceListViewModelBase<T>`` that adds the load, refresh and unload lifecycle, and selection.|
|[``PagedSourceListViewModelBase<T>``](ViewModels/Paging/PagedSourceListViewModelBase.cs)|``SourceListViewModelBase<T>`` that adds paging.|
|[``IPagedDataProvider<T>``](ViewModels/Paging/IPagedDataProvider.cs)|Source of paged data.|

### Collections

|class|summary|
|---|---|
|[``ObservableCircularBuffer<T>``](ObservableCircularBuffer.cs)|Fixed size collection, that adds new entries at the beginning and removes the last entry when it is full.|
|[``ObservableDictionary<TKey, TValue>``](ObservableDictionary.cs)|Dictionary that raises collection change notifications.|
|[``ObservableCollectionUpdater<T, TKey>``](ObservableCollectionUpdater.cs)|Synchronizes an ``ObservableCollection<T>`` with a new set of instances. An empty set counts as nothing to do and does not clear the collection.|

### State

|class|summary|
|---|---|
|[``BusyStack``](ViewModels/State/BusyStack.cs)|Threadsafe class that executes an action, providing information whether it currently contains any ``BusyToken`` when the Pull or Push methods are being called.|
|[``ObservableBusyStack``](ViewModels/State/ObservableBusyStack.cs)|``BusyStack`` that additionally provides means for observing other classes and being observed according to the ``IObserver<T>`` and ``IObservable<T>`` interfaces.|
|[``BusyToken``](ViewModels/State/BusyToken.cs)|A disposable struct thats being used by the ``IBusyStack`` implementations of this library.|
|[``NotifyProperyChangedTracker``](NotifyProperyChangedTracker.cs)|Change tracker based on ``INotifyPropertyChanged``. This class is not threadsafe.|
|[``PropertyChangedMessageTracker``](PropertyChangedMessageTracker.cs)|Change tracker listening for ``PropertyChangedMessage<T>`` from an ``IMessenger``, which provides old and new values.|
|[``DispatcherProgress<T>``](DispatcherProgress.cs)|``IProgress<T>`` that limits invocations of a callback on the dispatcher to a given interval.|

### Navigation

|class|summary|
|---|---|
|[``Scene``](ViewModels/Navigation/Scene.cs)|Simple viewmodel class that provides a localized name, its content and whether it is selected.|
|[``Scenes``](ViewModels/Navigation/Scenes.cs)|Abstract viewmodel collection class that holds instances of ``Scene``. Use it as a starting point for writing your own navigation system.|

### Localization

|class|summary|
|---|---|
|[``LocalizationViewModel``](ViewModels/Localization/LocalizationViewModel.cs)|Provides binding support for a localized string.|
|[``LocalizationsViewModel``](ViewModels/Localization/LocalizationsViewModel.cs)|Viewmodel providing binding support for the current language and the supported languages of a given ``ILocalizationProvider``.|

## Usage

### ViewModelBase

```cs
using MvvmScarletToolkit;
using MvvmScarletToolkit.Observables;

internal sealed class DerivedViewModel : ViewModelBase
{
    private string _name = string.Empty;
    public string Name
    {
        get { return _name; }
        set { SetProperty(ref _name, value); }
    }

    public DerivedViewModel(IScarletCommandBuilder commandBuilder)
        : base(commandBuilder)
    {
    }
}
```

### ViewModelBase\<TModel>

```cs
internal sealed record User(string Name);

internal sealed class UserViewModel : ViewModelBase<User>
{
    public UserViewModel(IScarletCommandBuilder commandBuilder, User model)
        : base(commandBuilder, model)
    {
    }
}
```

### BusinessViewModelBase

Implement how the content is refreshed and unloaded. `Load` calls `RefreshInternal` by default, and `LoadInternal` can be overridden to change that.

```cs
internal sealed class DetailViewModel : BusinessViewModelBase
{
    public DetailViewModel(IScarletCommandBuilder commandBuilder)
        : base(commandBuilder)
    {
    }

    protected override Task RefreshInternal(CancellationToken token)
    {
        // fetch the content here
        return Task.CompletedTask;
    }

    protected override Task UnloadInternal(CancellationToken token)
    {
        // release the content here
        return Task.CompletedTask;
    }
}
```

### BusinessViewModelListBase\<TViewModel>

```cs
internal sealed class UsersViewModel : BusinessViewModelListBase<UserViewModel>
{
    private readonly IUserService _users;

    public UsersViewModel(IScarletCommandBuilder commandBuilder, IUserService users)
        : base(commandBuilder)
    {
        _users = users;
    }

    protected override async Task RefreshInternal(CancellationToken token)
    {
        var users = await _users.GetAll(token);

        await Clear(token);
        await AddRange(users.Select(user => new UserViewModel(CommandBuilder, user)), token);
    }
}
```

### BusyStack

The action is called when the first token is created and when the last one is disposed:

```cs
var busyStack = new BusyStack(isBusy => Debug.WriteLine(isBusy));

using (busyStack.GetToken())
{
    // true has been reported while this block runs
}
```

### Change tracking

```cs
var tracker = new NotifyProperyChangedTracker();
tracker.Track(viewModel);

viewModel.Name = "changed";

var hasChanges = tracker.HasChanges(viewModel); // true

// changes made within this scope are not recorded
using (tracker.SuppressChanges(viewModel))
{
    viewModel.Name = "loaded from a file";
}

tracker.ClearChanges(viewModel);
```

### Navigation

Derive from `Scenes` and add your viewmodels. The key is translated through the `LocalizationsViewModel`:

```cs
internal sealed class ShellViewModel : Scenes
{
    public ShellViewModel(IScarletCommandBuilder commandBuilder, LocalizationsViewModel localizations, UsersViewModel users)
        : base(commandBuilder, localizations)
    {
        Add("Users", users);
    }
}
```
