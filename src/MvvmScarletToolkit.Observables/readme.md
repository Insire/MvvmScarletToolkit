# MvvmScarletToolkit.Observables

## Goals

This library aims to provide base and utility classes around state and state management in viewmodels in the MVVM pattern.

## Contents

|class|summary|
|---|---|
|[``ObservableCircularBuffer<T>``](ObservableCircularBuffer.cs)| Wraps a fixed sized ``ObservableCollection<T>`` and removes the last entry when a new entry is added in the beginning of the collection|

---

### ViewModels

|class|summary|
|---|---|
|[``ObservableObject``](ViewModels/Base/ObservableObject.cs)|General purpose implementation of the ``INotifyPropertyChanged`` interface|
|[``ViewModeBase``](ViewModels/Base/ViewModelBase.cs)|BaseViewModel that serves as service aggregate and caches INotifyPropertyChanged EventArgs
|[``ViewModeBase<T>``](ViewModels/Base/ViewModelBase_generic.cs)|Generic version of the ViewModelBase exposing an injected (model)class|
|[``ViewModeListBase<T>``](ViewModels/Base/ViewModelListBase.cs)|Base class that provides a common set of actions for a collection entity viewmodel centered around loading and unlad state and/or data.|
|[``ViewModelContainer<T>``](ViewModels/ViewModelContainer.cs)|Generic wrapper viewmodel to add binding support|
|[``BusinessViewModelBase``](ViewModels/Base/BusinessViewModelBase.cs)|ViewModelBase that bootstraps loading, unloading and refreshing of its content|
|[``BusinessViewModelBase<T>``](ViewModels/Base/BusinessViewModelBase.cs)|Generic ViewModelBase that bootstraps loading, unloading and refreshing of its content
|[``BusinessViewModelListBase<T>``](ViewModels/Base/BusinessViewModelListBase.cs)|Collection ViewModelBase that bootstraps loading, unloading and refreshing of its content|
|[``PagingViewModel``](ViewModels/PagingViewModel.cs)|ViewModel that adds paging support to ``DomainViewModelListBase<TViewModel>``|

#### Usage

##### ObservableObject

```cs
using MvvmScarletToolkit.Observables;

// DerivedObservableObject.cs
internal sealed class DerivedObservableObject : ObservableObject
{
    private object _notifyingProperty;
    public object NotifyingProperty
    {
        get { return _notifyingProperty; }
        set { SetValue(ref _notifyingProperty, value, onChanged: OnChanged, onChanging: OnChanging); }
    }

    private void OnChanged()
    {
        // your code here, when NotifyingProperty changed
    }

    private void OnChanging()
    {
        // your code here, when NotifyingProperty is about to change
    }
}
```

##### ViewModelBase

```cs
using MvvmScarletToolkit.Observables;

// DerivedViewModelBase.cs
internal sealed class DerivedViewModelBase : ViewModelBase
{
    public DerivedViewModelBase(IScarletCommandBuilder commandBuilder)
        : base(commandBuilder)
    {
    }
}
```

##### ViewModelBase\<TClass>

```cs
using MvvmScarletToolkit.Observables;

// TClass is equivalent to any class
// DerivedObjectViewModelBase.cs
internal sealed class DerivedObjectViewModelBase : ViewModelBase<TClass>
{
    public DerivedObjectViewModelBase(IScarletCommandBuilder commandBuilder, TClass model)
        : base(commandBuilder)
    {
    }
}
```

---

### State

|class|summary|
|---|---|
|[``BusyStack``](ViewModels/State/BusyStack.cs)|Threadsafe class that executes an action, providing information whether it currently contains any ``BusyToken`` when the Pull or Push methods are being called.|
|[``ObservableBusyStack``](ViewModels/State/ObservableBusyStack.cs)|Threadsafe class that executes an action, providing information whether it currently contains any ``BusyToken`` when the Pull or Push methods are being called. Additionally provides means for observing other classes and being observed according to the ``IObserver<T>`` and ``IObservable<T>`` interfaces.|
|[``BusyToken``](ViewModels/State/BusyToken.cs)|A a disposable class thats being used by the ``IBusyStack`` implementations of this library|

---

### Navigation

|class|summary|
|---|---|
|[``Scene``](ViewModels/Navigation/Scene.cs)|Simple viewmodel class that provides a viewmodel property and whether it is selected|
|[``Scenes``](ViewModels/Navigation/Scenes.cs)|Abstract viewmodel collection class that holds instances of ``Scene``s. Usecase is for having a starting point to writing your own navigation system. |

---

### Localization

|class|summary|
|---|---|
|[``LocalizationViewModel``](ViewModels/Localization/LocalizationViewModel.cs)|Provides binding support for a localized string|
|[``LocalizationsViewModel``](ViewModels/Localization/LocalizationsViewModel.cs)|Viewmodel providing binding support for current language and support languages of a given ``ILocalizationProvider``|
