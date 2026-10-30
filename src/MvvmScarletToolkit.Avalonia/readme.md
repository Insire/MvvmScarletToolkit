# MvvmScarletToolkit.Avalonia

## Goals

This library aims to provide implementations for avalonia specific classes used by the framework.

## Features

- **Toolkit services for Avalonia:** the dispatcher, command manager, exit service and weak event manager, which `ScarletCommandBuilder.Default` combines. Viewmodels and commands work the same as on WPF.
- **Unload on exit:** business viewmodels are unloaded when a classic desktop application shuts down. Other application lifetimes are not supported by `ScarletExitService`.
- **Async image loading:** `AsyncImageLoadingBehavior` loads images through [MvvmScarletToolkit.ImageLoading](../MvvmScarletToolkit.ImageLoading/readme.md).
- **Converters as markup extensions:** `ConverterMarkupExtension<T>` is a base class for writing converters that don't have to be declared as resources.

The behaviors, converters and controls of [MvvmScarletToolkit.Wpf](../MvvmScarletToolkit.Wpf/readme.md) are not available for Avalonia.

## Contents

|class|summary|
|---|---|
|[``ScarletCommandBuilder``](./Implementations/ScarletCommandBuilder.cs)|Fluent builder for ConcurrentCommand instances.|
|[``ScarletCommandManager``](./Implementations/ScarletCommandManager.cs)|Raises ``RequerySuggested``, so that commands can update their ``CanExecute`` state.|
|[``ScarletDispatcher``](./Implementations/ScarletDispatcher.cs)|Wrapper for the Avalonia dispatcher to support Dependency Injection and Task based ICommand implementations in this library.|
|[``ScarletExitService``](./Implementations/ScarletExitService.cs)|Takes care of executing ``IVirtualizationViewModel.UnloadCommand`` when the application exits.|
|[``ScarletLocalizationProvider``](./Implementations/ScarletLocalizationProvider.cs)|Dummy Implementation of ``ILocalizationProvider``.|
|[``ScarletWeakEventManager``](./Implementations/ScarletWeakEventManager.cs)|Weak event subscriptions for ``INotifyPropertyChanged``, to support Dependency Injection.|
|[``AvaloniaImageFactory``](./Implementations/AvaloniaImageFactory.cs)|``IImageFactory<Bitmap>`` for [MvvmScarletToolkit.ImageLoading](../MvvmScarletToolkit.ImageLoading/readme.md). Scales images down to the requested size, keeping their aspect ratio.|
|[``AsyncImageLoadingBehavior``](./Behaviors/AsyncImageLoadingBehavior.cs)|Attached properties for loading the source of an ``Image`` asynchronously.|
|[``ConverterMarkupExtension<T>``](./ConverterMarkupExtension.cs)|Base class for writing your own converters as markup extensions.|

## Usage

### Startup

```cs
public override void OnFrameworkInitializationCompleted()
{
    if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
    {
        desktop.MainWindow = new MainWindow
        {
            DataContext = new MainViewModel(ScarletCommandBuilder.Default),
        };
    }

    base.OnFrameworkInitializationCompleted();
}
```

### Image loading

Assign the image service once at startup, as described in [MvvmScarletToolkit.ImageLoading](../MvvmScarletToolkit.ImageLoading/readme.md), then set the attached properties on an `Image`:

```cs
AsyncImageLoadingBehavior.Loader = serviceProvider.GetRequiredService<Lazy<IImageService<Bitmap>>>();
```

```xml
xmlns:mvvm="http://SoftThorn.MvvmScarletToolkit.com/winfx/xaml/shared"
```

```xml
<Image mvvm:AsyncImageLoadingBehavior.Source="{Binding Url}"
       mvvm:AsyncImageLoadingBehavior.Width="300"
       mvvm:AsyncImageLoadingBehavior.Height="300" />
```

`State`, `IsPending`, `IsLoading`, `IsLoaded` and `IsFailed` are also available as attached properties, for example to show a placeholder.

### Converters

```cs
using MvvmScarletToolkit.Avalonia;
using System.Globalization;

public sealed class IsNot : ConverterMarkupExtension<IsNot>
{
    public override object? Convert(object? value, Type? targetType, object? parameter, CultureInfo? culture)
    {
        return value is bool boolean ? !boolean : value;
    }
}
```

```xml
<Button IsEnabled="{Binding IsBusy, Converter={local:IsNot}}" />
```
