# MvvmScarletToolkit.Wpf

## Goals

This library aims to provide implementations for wpf specific classes used by the framework, and a collection of behaviors, converters and controls for WPF applications.

## Setup

All XAML types are available through a single namespace. Behaviors also need the namespace of [Microsoft.Xaml.Behaviors.Wpf](https://github.com/microsoft/XamlBehaviorsWpf):

```xml
xmlns:i="http://schemas.microsoft.com/xaml/behaviors"
xmlns:mvvm="http://SoftThorn.MvvmScarletToolkit.com/winfx/xaml/shared"
```

Toast notifications and the FileSystemBrowser come with default styles. Merge them in `App.xaml` when using those features:

```xml
<ResourceDictionary Source="pack://application:,,,/MvvmScarletToolkit.Wpf;component/Features/ToastNotification/Generic.xaml" />
<ResourceDictionary Source="pack://application:,,,/MvvmScarletToolkit.Wpf;component/Features/FileSystemBrowser/Generic.xaml" />
```

## Features

### Controls

|name|summary|
|---|---|
|[FileSystemBrowser](Features/FileSystemBrowser/readme.md)|Browse drives, folders and files through a viewmodel, with file and folder pickers.|
|``VirtualizingTilePanel``|Virtualizing panel that lays out items as tiles of ``ChildSize``.|
|``VirtualizingWrapPanel``|Virtualizing panel that wraps items of ``ItemWidth`` and ``ItemHeight``.|
|``HtmlTextBlock``|TextBlock that displays formatted text from its ``Html`` property, like ``b``, ``i``, ``u``, ``sub``, ``sup``, ``a`` and ``br``.|
|``HtmlHighlightTextBlock``|``HtmlTextBlock`` that also highlights the text from its ``Highlight`` property, e.g. for search results.|

### Toast notifications

``ToastService`` shows notifications in their own window, without needing a reference to a view. ``ToastServiceConfiguration`` sets the window style, position and durations.

```cs
ToastService.Default.Show("Saved", "Your changes have been saved.", ToastType.Success);
```

Pass ``true`` as ``isPersistent`` to keep a toast visible until the user closes it:

```cs
ToastService.Default.Show("Error", "The file could not be saved.", ToastType.Error, true);
```

### Grouping

``GroupingViewModel`` manages the groups of an ``ICollectionView`` from a viewmodel, so that users can add and remove groupings at runtime. ``GroupingViewModel.Create`` builds one for a collection.

### GridView column sizing

Adds proportional and limited column widths to a ``GridView``, like ``*`` sizing in a ``Grid``. Enable it with ``ListViewLayoutManager.Enabled`` on a ``ListView``, then set the width of each column with the attached properties of these classes:

|class|summary|
|---|---|
|``FixedColumn``|Column with a fixed width.|
|``ProportionalColumn``|Column taking a share of the available width.|
|``RangeColumn``|Column limited by ``MinWidth`` and ``MaxWidth``, that can fill the remaining space with ``IsFillColumn``.|

### DataTemplate selection

``TypeDataTemplateSelector`` checks an explicit list of datatemplates for a fitting type, before falling back to WPF's default lookup. Its ``DataTemplateCollection`` allows one entry per ``DataType``.

### Behaviors

|behavior|attach to|summary|
|---|---|---|
|``AllowableCharactersTextBoxBehavior``|``TextBox``|Limit input by ``MaxLength``, ``AllowWhitespace`` and ``RegularExpression``. The expression is matched against the typed or pasted text, so anchor it with ``^`` and ``$``.|
|``AsyncImageLoadingBehavior``|``Image``|Load an image through [MvvmScarletToolkit.ImageLoading](../MvvmScarletToolkit.ImageLoading/readme.md), and report ``State``, ``IsPending``, ``IsLoading``, ``IsLoaded`` and ``IsFailed``.|
|``AutoRepositionPopupBehavior``|``Popup``|Keep a popup at its position, when its window is moved.|
|``AutoScrollBehavior``|``Selector``|Scroll to the end of e.g. a ``ListBox`` or ``DataGrid``. Pauses when the user scrolls manually.|
|``LaunchNavigateUriAsNewProcessBehavior``|``Hyperlink``|Open ``Hyperlink.NavigateUri`` in a new process.|
|``MultiListBoxSelectionBehavior``|``ListBox``|Bind ``ListBox.SelectedItems`` to an existing ``IList``.|
|``MultiSelectionBehavior``|``MultiSelector``|Bind ``SelectedItems`` of e.g. a ``DataGrid`` to an existing ``IList``.|
|``PasswordBindingBehavior``|``PasswordBox``|Bind the password through ``ClearTextPassword``.|
|``ScrollSelectionIntoViewBehavior``|``Selector``|Scroll the selected item of a ``ListBox`` or ``DataGrid`` into view.|
|``ScrollToEndCommandBehavior``|``FrameworkElement``|Execute ``Command`` when a scrollviewer reaches its vertical or horizontal end, sampled at ``Interval``. Useful for loading the next page.|
|``SelectedTreeViewItemBehavior``|``TreeView``|Enable twoway binding of the selected item.|
|``SmoothProgressBehavior``|``ProgressBar``|Animate changes of ``Percentage``, instead of jumping to the new value.|
|``WatermarkBehavior``|``TextBox``|Display ``Watermark`` on top of a ``TextBox`` while it is empty and not focused. Customize it with ``WatermarkStyle``.|

```xml
<PasswordBox>
    <i:Interaction.Behaviors>
        <mvvm:PasswordBindingBehavior ClearTextPassword="{Binding Password}" />
    </i:Interaction.Behaviors>
</PasswordBox>

<TextBox>
    <i:Interaction.Behaviors>
        <mvvm:WatermarkBehavior Watermark="Search" />
        <mvvm:AllowableCharactersTextBoxBehavior MaxLength="4" RegularExpression="^[0-9]*$" />
    </i:Interaction.Behaviors>
</TextBox>

<ListBox ItemsSource="{Binding Items}">
    <i:Interaction.Behaviors>
        <mvvm:ScrollToEndCommandBehavior Command="{Binding NextCommand}" Interval="00:00:00.250" />
    </i:Interaction.Behaviors>
</ListBox>
```

### Attached properties

|property|summary|
|---|---|
|``Filter.By``|Bind a ``Predicate<object>`` as the filter of an ``ICollectionView``. Can be set on a ``CollectionViewSource`` or an ``ItemsControl``.|
|``Filter.RefreshWhenChanged``|Refresh the ``ICollectionView`` when the bound value changes, e.g. a search text.|
|``Focus.First``|Move the focus to the control with the lowest tab order on load. A WPF window has no focus when it is first shown.|
|``ImageLoader.Source``|Attached alternative to ``AsyncImageLoadingBehavior``, with ``ImageLoader.IsLoading`` reporting its state.|

```xml
<ListBox ItemsSource="{Binding Items}"
         mvvm:Filter.By="{Binding Filter}"
         mvvm:Filter.RefreshWhenChanged="{Binding FilterText}" />
```

### TriggerActions

|action|summary|
|---|---|
|``ClearTextBoxAction``|Clear the ``TextBox`` set as ``Target``, e.g. from a clear button.|
|``ClearPasswordBoxAction``|Clear the ``PasswordBox`` set as ``Target``.|

### Converters

All converters are markup extensions, so they don't have to be declared as resources:

```xml
<TextBlock Visibility="{Binding IsBusy, Converter={mvvm:BooleanToVisibility}}" />
<Button IsEnabled="{Binding Name, Converter={mvvm:IsNotNullOrWhiteSpace}}" />
<TextBlock Text="{Binding Name, Converter={mvvm:ToCase}}" />
```

|converter|summary|
|---|---|
|``BooleanToVisibility``|Convert a boolean to visibility.|
|``InvertBooleanToVisibility``|Convert a boolean to the inverted visibility.|
|``IsNot``|Negate a boolean.|
|``IsNull``, ``IsNotNull``|Whether something is null or not.|
|``IsNullOrEmpty``, ``IsNotNullOrEmpty``|Whether a string is null or empty, or not.|
|``IsNullOrWhiteSpace``, ``IsNotNullOrWhiteSpace``|Whether a string is null or whitespace, or not.|
|``GreaterThan``, ``SmallerThan``|Compare a bound number with ``Value`` or the converter parameter.|
|``ToCase``|Change the casing of a string with ``Casing`` or the converter parameter. Defaults to upper case.|
|``Flatten``|Join an enumerable of strings into one string with ``Separator``, skipping empty entries.|
|``IgnoreNullOrEmptyString``|Return ``DependencyProperty.UnsetValue`` for empty strings, so that a binding uses its fallback value.|
|``RadioButtonCheckedConverter``|Compare the bound value with the converter parameter, to bind radio buttons to a single value.|
|``MultiBooleanAndConverter``|Combine all values of a ``MultiBinding`` with a logical and.|
|``DebugConverter``|Log bound values and value changes to the debug output.|

### MarkupExtensions

|extension|summary|
|---|---|
|``EnumBindingSource``|Create an items source from an enum type. ``FilterNonBrowseables`` skips entries hidden with ``[Browsable(false)]``.|
|``StartProcess``|Provide a command that lets the OS open a url.|
|``ConverterMarkupExtension<T>``|Base class for writing your own converters as markup extensions.|
|``MultiConverterMarkupExtension<T>``|Base class for writing your own multi value converters as markup extensions.|

```xml
<ComboBox ItemsSource="{mvvm:EnumBindingSource {x:Type mvvm:ToastType}}" />
<Hyperlink Command="{mvvm:StartProcess Url='https://github.com/Insire/MvvmScarletToolkit'}" />
```

### Other

|name|summary|
|---|---|
|``BindingProxy``|Forward a ``DataContext`` to elements outside of the visual tree, through its ``Data`` property.|
|``DependencyObjectExtensions``|Find parents, children and datatemplates in the visual tree.|

## Contents

These classes implement the services used by [``IScarletCommandBuilder``](../MvvmScarletToolkit.Commands/readme.md). ``ScarletCommandBuilder.Default`` combines them.

|class|summary|
|---|---|
|[``ScarletCommandBuilder``](./Implementations/ScarletCommandBuilder.cs)|Fluent builder for ConcurrentCommand instances.|
|[``ScarletCommandManager``](./Implementations/ScarletCommandManager.cs)|Wrapper for [System.Windows.Input.CommandManager](https://docs.microsoft.com/en-gb/dotnet/api/system.windows.input.commandmanager) to support Dependency Injection.|
|[``ScarletDispatcher``](./Implementations/ScarletDispatcher.cs)|Wrapper for [System.Windows.Threading.Dispatcher](https://docs.microsoft.com/en-us/dotnet/api/system.windows.threading.dispatcher) to support Dependency Injection and Task based ICommand implementations in this library.|
|[``ScarletExitService``](./Implementations/ScarletExitService.cs)|In WPF ui controls dont always raise their Unloaded-event, when the application is being closed. ScarletExitService takes care of executing ``IVirtualizationViewModel.UnloadCommand`` when the application exits.|
|[``ScarletLocalizationProvider``](./Implementations/ScarletLocalizationProvider.cs)|Dummy Implementation of ``ILocalizationProvider``, which returns keys unchanged and offers no languages.|
|[``ScarletWeakEventManager``](./Implementations/ScarletWeakEventManager.cs)|Wrapper for [System.Windows.WeakEventManager](https://docs.microsoft.com/en-us/dotnet/api/system.windows.weakeventmanager) to support Dependency Injection.|
|[``WpfImageFactory``](./Implementations/WpfImageFactory.cs)|``IImageFactory<BitmapSource>`` for [MvvmScarletToolkit.ImageLoading](../MvvmScarletToolkit.ImageLoading/readme.md). Scales images down to the requested size, keeping their aspect ratio.|
