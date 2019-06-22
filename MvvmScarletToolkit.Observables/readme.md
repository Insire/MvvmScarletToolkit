[back](../readme.md)

# MvvmScarletToolkit.Observables

## Goals

This library aims to provide base and utility classes around state and state management in viewmodels in the MVVM pattern.

## Contents

|class|summary|code|
|---|---|---|
|``ObservableCircularBuffer<T>``| Wraps a fixed sized ``ObservableCollection<T>`` and removes the last entry when a new entry is added in the beginning of the collection|[``ObservableCircularBuffer<T>``](ObservableCircularBuffer.cs)|
|``RangeObservableCollection<T>``|Extends an ``ObservableCollection<T>`` to be able to add ranges of entries to it|[``RangeObservableCollection<T>``](RangeObservableCollection.cs)|

[Samples](samples.md)

---

### ViewModels

|class|summary|code|
|---|---|---|
|``ObservableObject``|General purpose implementation of the ``INotifyPropertyChanged`` interface|[``ObservableObject``](ViewModels\ObservableObject.cs)
|``ViewModeBase<T>``|Base class that provides a common set of actions for a single entity viewmodel centered around loading and unloading state and/or data.|[``ViewModeBase<T>``](ViewModels\ViewModelBase.cs)
|``ViewModeListBase<T>``|Base class that provides a common set of actions for a collection entity viewmodel centered around loading and unlad state and/or data.|[``ViewModeListBase<T>``](ViewModels\ViewModelListBase.cs)
|``FilterViewModel<T>``|Provides a generic approach to enable filtering for a bound collection.|[``FilterViewModel<T>``](ViewModels\FilterViewModel.cs)
|``ViewModelContainer<T>``|TODO|[``ViewModelContainer<T>``](ViewModels\ViewModelContainer.cs)
|``BusinessViewModelBase``|TODO|[``BusinessViewModelBase``](ViewModels\BusinessViewModelBase.cs)
|``BusinessViewModelBase<T>``|TODO|[``BusinessViewModelBase<T>``](ViewModels\BusinessViewModelBase.cs)
|``BusinessViewModelListBase<T>``|TODO|[``BusinessViewModelListBase<T>``](ViewModels\BusinessViewModelListBase.cs)

[Samples](ViewModels\samples.md)

---

### State

|class|summary|code|
|---|---|---|
|``BusyStack``|Threadsafe class that executes an action, providing information whether it currently contains any ``BusyToken`` when the Pull or Push methods are being called.|[``BusyStack``](ViewModels\State\BusyStack.cs)
|``ObservableBusyStack``|Threadsafe class that executes an action, providing information whether it currently contains any ``BusyToken`` when the Pull or Push methods are being called. Additionally provides means for observing other classes and being observed according to the ``IObserver<T>`` and ``IObservable<T>`` interfaces.|[``ObservableBusyStack``](ViewModels\State\ObservableBusyStack.cs)
|``BusyToken``|A a disposable class thats being used by the ``IBusyStack`` implementations of this library|[``BusyToken``](ViewModels\State\BusyToken.cs)
|``ChangeTracker``|TODO|[``ChangeTracker``](ChangeTracker.cs)

[Samples](ViewModels\State\samples.md)

---

### Navigation

|class|summary|code|
|---|---|---|
|``Scene``|Simple viewmodel class that provides a viewmodel property and whether it is selected|[``Scene``](ViewModels\Navigation\Scene.cs)
|``Scenes``|Abstract viewmodel collection class that holds instances of ``Scene``s. Usecase is for having a starting point to writing your own navigation system. |[``Scenes``](ViewModels\Navigation\Scenes.cs)

[Samples](ViewModels\Navigation\samples.md)

---

### Localization

|class|summary|code|
|---|---|---|
|``LocalizationViewModel``|TODO|[``LocalizationViewModel``](ViewModels\Localization\LocalizationViewModel.cs)
|``LocalizationsViewModel``|TODO|[``LocalizationsViewModel``](ViewModels\Localization\LocalizationsViewModel.cs)

[Samples](ViewModels\Localization\samples.md)
