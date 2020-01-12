# MvvmScarletToolkit.Observables

## Goals

This library aims to provide base and utility classes around state and state management in viewmodels in the MVVM pattern.

## Contents

|class|summary|
|---|---|
|[``ObservableCircularBuffer<T>``](ObservableCircularBuffer.cs)| Wraps a fixed sized ``ObservableCollection<T>`` and removes the last entry when a new entry is added in the beginning of the collection|

[Samples](samples.md)

---

### ViewModels

|class|summary|
|---|---|
|[``ObservableObject``](ViewModels\Base\ObservableObject.cs)|General purpose implementation of the ``INotifyPropertyChanged`` interface|
|[``ViewModeBase<T>``](ViewModels\Base\ViewModelBase.cs)|Base class that provides a common set of actions for a single entity viewmodel centered around loading and unloading state and/or data.|
|[``ViewModeListBase<T>``](ViewModels\Base\ViewModelListBase.cs)|Base class that provides a common set of actions for a collection entity viewmodel centered around loading and unlad state and/or data.|
|[``ViewModelContainer<T>``](ViewModels\ViewModelContainer.cs)|Generic wrapper viewmodel to add binding support|
|[``BusinessViewModelBase``](ViewModels\Base\BusinessViewModelBase.cs)|ViewModelBase that bootstraps loading, unloading and refreshing of its content|
|[``BusinessViewModelBase<T>``](ViewModels\Base\BusinessViewModelBase.cs)|Generic ViewModelBase that bootstraps loading, unloading and refreshing of its content
|[``BusinessViewModelListBase<T>``](ViewModels\Base\BusinessViewModelListBase.cs)|Collection ViewModelBase that bootstraps loading, unloading and refreshing of its content|
|[``PagingViewModel``](ViewModels\PagingViewModel.cs)|ViewModel that adds paging support to ``DomainViewModelListBase<TViewModel>``|

[Samples](ViewModels\samples.md)

---

### State

|class|summary|
|---|---|
|[``BusyStack``](ViewModels\State\BusyStack.cs)|Threadsafe class that executes an action, providing information whether it currently contains any ``BusyToken`` when the Pull or Push methods are being called.|
|[``ObservableBusyStack``](ViewModels\State\ObservableBusyStack.cs)|Threadsafe class that executes an action, providing information whether it currently contains any ``BusyToken`` when the Pull or Push methods are being called. Additionally provides means for observing other classes and being observed according to the ``IObserver<T>`` and ``IObservable<T>`` interfaces.|
|[``BusyToken``](ViewModels\State\BusyToken.cs)|A a disposable class thats being used by the ``IBusyStack`` implementations of this library|

[Samples](ViewModels\State\samples.md)

---

### Navigation

|class|summary|
|---|---|
|[``Scene``](ViewModels\Navigation\Scene.cs)|Simple viewmodel class that provides a viewmodel property and whether it is selected|
|[``Scenes``](ViewModels\Navigation\Scenes.cs)|Abstract viewmodel collection class that holds instances of ``Scene``s. Usecase is for having a starting point to writing your own navigation system. |

[Samples](ViewModels\Navigation\samples.md)

---

### Localization

|class|summary|
|---|---|
|[``LocalizationViewModel``](ViewModels\Localization\LocalizationViewModel.cs)|Provides binding support for a localized string|
|[``LocalizationsViewModel``](ViewModels\Localization\LocalizationsViewModel.cs)|Viewmodel providing binding support for current language and support languages of a given ``ILocalizationProvider``|

[Samples](ViewModels\Localization\samples.md)

[back](../readme.md)
