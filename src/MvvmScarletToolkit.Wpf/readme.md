![MvvmScarletToolkit.Wpf](https://raw.githubusercontent.com/Insire/MvvmScarletToolkit/master/assets/branding/nuget/MvvmScarletToolkit.Wpf.png)

## Goals

This library aims to provide implementations for wpf specific classes used by the framework, and a collection of behaviors, converters and controls for WPF applications.

## Setup

All XAML types are available through a single namespace. Behaviors also need the namespace of [Microsoft.Xaml.Behaviors.Wpf](https://github.com/microsoft/XamlBehaviorsWpf):

```xml
xmlns:i="http://schemas.microsoft.com/xaml/behaviors"
xmlns:mvvm="http://SoftThorn.MvvmScarletToolkit.com/winfx/xaml/shared"
```

Toast notifications, the FileSystemBrowser and the DataGrid extensions come with default styles. Merge them in `App.xaml` when using those features:

```xml
<ResourceDictionary Source="pack://application:,,,/MvvmScarletToolkit.Wpf;component/Features/DataGridExtensions/Generic.xaml" />
<ResourceDictionary Source="pack://application:,,,/MvvmScarletToolkit.Wpf;component/Features/ToastNotification/Generic.xaml" />
<ResourceDictionary Source="pack://application:,,,/MvvmScarletToolkit.Wpf;component/Features/FileSystemBrowser/Generic.xaml" />
```

## Features

### Controls

|name|summary|
|---|---|
|[FileSystemBrowser](Features/FileSystemBrowser/readme.md)|Browse drives, folders and files through a viewmodel, with file and folder dialogs.|
|``VirtualizingTilePanel``|Virtualizing panel that lays out items as tiles of ``ChildSize``.|
|``VirtualizingWrapPanel``|Virtualizing panel that wraps items of ``ItemWidth`` and ``ItemHeight``.|
|``HtmlTextBlock``|TextBlock that displays formatted text and images from its ``Html`` property. See [HtmlTextBlock](#htmltextblock).|
|``HtmlHighlightTextBlock``|``HtmlTextBlock`` that also highlights the text from its ``Highlight`` property, e.g. for search results.|

### HtmlTextBlock

``HtmlTextBlock`` supports a small subset of html. Attribute values can be double quoted, single quoted or unquoted. Entities like ``&lt;``, ``&amp;``, ``&nbsp;`` and ``&#39;`` are decoded in text and attribute values.

Unlike a browser, ``HtmlTextBlock`` keeps whitespace and line breaks of the text as they are, and block tags like headings and list items don't start a new line on their own. Put them on their own line in the html, or use ``br``.

|tag|effect|
|---|---|
|``b``, ``i``, ``u``, ``sub``, ``sup``|Bold, italic, underlined, subscript and superscript text.|
|``h1`` to ``h6``|Bold text, scaled from 2 to 0.9 times the font size of the control at the time the html is set.|
|``code``, ``pre``|Monospace text.|
|``ul``, ``ol``, ``li``|Bullets or numbers in front of each ``li``, nested lists are indented.|
|``a href="…"``|Hyperlink. Set ``OpenLinksInBrowser="True"`` to open clicked http, https and mailto links with the default application. Other links are disabled then, since a ``file:`` link could start any program.|
|``br``|Line break.|
|``font color="…"``, ``color color="…"``, ``span style="color: …"``|Text color: named colors, ``#rgb``, ``#rrggbb`` or ``#aarrggbb``. The innermost color wins, invalid colors are ignored.|
|``img``|Inline image, see below.|

``img`` reads these attributes:

|attribute|effect|
|---|---|
|``src``|Image source. ``source`` still works, ``src`` wins if both are set.|
|``width``, ``height``|Size, as a plain number or in ``px``.|
|``alt``|Name for screen readers. Shown instead of the image, when it cannot be loaded.|
|``title``|Tooltip.|
|``href``|Makes the image a hyperlink.|
|``style``|``width``, ``height``, ``margin`` (1 to 4 values), ``vertical-align`` (``baseline``, ``top``, ``middle``, ``bottom``, ``text-top``, ``text-bottom``) and ``opacity``. Lengths are plain numbers or ``px``. The style wins over the ``width`` and ``height`` attributes.|

```xml
<mvvm:HtmlTextBlock Html="Saved &lt;img src='https://example.com/check.png' alt='done' style='width: 16px; height: 16px; vertical-align: middle'&gt; &lt;font color='Green'&gt;successfully&lt;/font&gt;" />
```

Images with http(s), file and relative sources load through an ``IImageService<BitmapSource>`` from [MvvmScarletToolkit.ImageLoading](https://github.com/Insire/MvvmScarletToolkit/blob/master/src/MvvmScarletToolkit.ImageLoading/readme.md), so they are cached and load asynchronously. ``HtmlTextBlock.ImageService`` sets the service for one control, otherwise ``ImageLoader.AsyncImageLoader`` is used. When both width and height are known in px, the image is decoded at that size. Without a service, and for other sources like ``pack://`` uris, images are loaded with a ``BitmapImage``.

> Html from untrusted sources can make the control download images from any web address.

### Selecting text

``TextBlockSelection.IsEnabled`` makes the text of any ``TextBlock`` selectable, including ``HtmlTextBlock``. Users select text by dragging the mouse, double-clicking selects a word, ``Ctrl+A`` selects everything and ``Ctrl+C`` copies. A context menu with both commands is added, unless you set your own ``ContextMenu``.

```xml
<TextBlock mvvm:TextBlockSelection.IsEnabled="True" Text="{Binding Message}" />

<Style TargetType="{x:Type TextBlock}">
    <Setter Property="mvvm:TextBlockSelection.IsEnabled" Value="True" />
    <Setter Property="mvvm:TextBlockSelection.SelectionBrush" Value="Orange" />
    <Setter Property="mvvm:TextBlockSelection.SelectionOpacity" Value="0.6" />
</Style>
```

``IsEnabled`` adds a ``TextBlockSelectionBehavior``, which can also be added directly, as a style can't do that:

```xml
<TextBlock Text="{Binding Message}">
    <i:Interaction.Behaviors>
        <mvvm:TextBlockSelectionBehavior />
    </i:Interaction.Behaviors>
</TextBlock>
```

``SelectionBrush``, ``SelectionOpacity`` and the read-only ``SelectedText`` are attached properties of ``TextBlockSelection`` either way. Bind to the selected text with ``{Binding (mvvm:TextBlockSelection.SelectedText), ElementName=Name}``. From code, the behavior offers ``Select``, ``SelectWord``, ``SelectAll``, ``ClearSelection`` and ``Copy``.

The selection ends when the ``TextBlock`` loses the focus or its text changes. It is drawn in an adorner on top of the text, so inlines keep their foreground and bindings, and the highlight needs an ``AdornerLayer`` to show up. Every ``Window`` and ``ScrollViewer`` has one.

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

``GroupingViewModel`` manages the groups of an ``ICollectionView`` from a viewmodel, so that users can add and remove groupings at runtime. ``GroupingViewModel.Create`` builds one for a collection. ``GroupBy`` and ``Ungroup`` change groupings from code.

Every public property is offered for grouping by default. Pass ``GroupingViewModelOptions`` to change that:

```cs
var groups = GroupingViewModel.Create(commandBuilder, Items, new GroupingViewModelOptions
{
    ExcludeToolkitProperties = true, // hides properties like IsBusy
    Properties = ["Color", "Size"], // only offers these
});
```

### DataGrid extensions

Everything here works through behaviors and commands, so viewmodels don't need to know about the grid. Merge the [default styles](#setup) to use the keyed styles below.

```xml
<ItemsControl x:Name="GroupPanel"
              DataContext="{Binding Groups}"
              Style="{StaticResource ScarletDataGridGroupPanelStyle}" />

<DataGrid x:Name="Grid"
          ColumnHeaderStyle="{StaticResource ScarletDataGridFilterColumnHeaderStyle}"
          ItemsSource="{Binding Items}">
    <i:Interaction.Behaviors>
        <mvvm:DataGridColumnChooserBehavior />
        <mvvm:DataGridLayoutBehavior Layout="{Binding Layout}" />
        <mvvm:DataGridFilterBehavior Filter="{Binding Filter}" />
        <mvvm:DataGridGroupingBehavior DropTarget="{Binding ElementName=GroupPanel}" Grouping="{Binding Groups}" />
    </i:Interaction.Behaviors>
    <DataGrid.Columns>
        <DataGridTextColumn mvvm:DataGridSummaryBar.Function="Sum"
                            mvvm:DataGridSummaryBar.Format="N2"
                            Binding="{Binding Price}"
                            Header="Price" />
    </DataGrid.Columns>
</DataGrid>

<mvvm:DataGridSummaryBar DataGrid="{Binding ElementName=Grid}" />
```

- **Columns**: ``DataGridColumnChooserBehavior`` adds a context menu to the column headers to hide and show columns.
- **Layout**: ``DataGridLayoutBehavior`` keeps a ``DataGridLayout`` in sync with visibility, order, width, sorting and summary function of the columns. See [Saving the layout](#saving-the-layout).
- **Filtering**: ``DataGridFilterViewModel.Create<T>`` offers one ``ColumnFilterViewModel`` per column, combined with AND. ``ScarletDataGridFilterColumnHeaderStyle`` adds a button to each header that opens a popup with the filter, which is applied when the popup closes. From code, set ``Operator`` and ``Value`` and call ``Apply``. The popup uses system colors by default. For another theme, add a style for ``DataGridColumnFilter`` based on ``ScarletDataGridColumnFilterStyle`` that sets ``Background`` and ``Foreground``. ``Operators`` only offers what fits the type of the property: comparisons for numbers, dates and times, equality for bool and enum, contains and equality for ``Guid`` and text. ``AdditionalFilter`` adds a predicate of your own. As ``Filter.By`` only holds one predicate, use this instead of setting it yourself.
- **Filter validation**: text is checked against the type of the property while typing, and an invalid value disables the filter. Add a WPF ``ValidationRule`` to ``ValidationRules`` for custom rules. Set ``IsValidationEnabled`` to false, on a ``ColumnFilterViewModel`` or on the ``DataGridFilterViewModel`` for all columns, to turn this off. There are no errors then, and a value that can't be converted is compared by its text.
- **Culture**: filters and summaries use the ``Language`` of the grid, like the bindings of its cells, and update when it changes. So what a user types and reads is always in the format the cells show. ``Language`` is en-US unless your application changes it, so call ``FrameworkElement.LanguageProperty.OverrideMetadata`` at startup to show values in the culture of the user, as the samples do. Without ``DataGridFilterBehavior``, set ``DataGridFilterViewModel.Culture`` yourself. A ``ConverterCulture`` on the binding of a column is not taken into account.
- **Grouping**: ``DataGridGroupingBehavior`` groups by a column when its header is dragged out of the header row and onto ``DropTarget``. Columns need a ``SortMemberPath``. Dragging along the headers still reorders columns.
- **Summaries**: ``DataGridSummaryBar`` shows ``Sum``, ``Average``, ``Minimum``, ``Maximum`` or ``Count`` over the filtered rows below the grid, with a tooltip that says what each value means. Users can change the function of a column at runtime, see [Changing summaries at runtime](#changing-summaries-at-runtime). The bar updates when rows are added, removed, filtered or edited through a cell. Call ``Refresh`` after changing a row in another way. Frozen columns are not supported.
- **Export**: provide an ``IDataGridExporter`` to turn the content of a grid into anything you like. See [Exporting a DataGrid](https://github.com/Insire/MvvmScarletToolkit/blob/master/docs/datagrid-export.md).

#### Changing summaries at runtime

``DataGridSummaryBar.Function`` sets the starting function of a column. Users can then right-click any cell of the bar and pick another function, or add one to a column that has none. Only that cell is calculated again, and the current function is checked in the menu.

The menu offers what fits the type of the property:

|type|functions|
|---|---|
|numbers|``Sum``, ``Average``, ``Minimum``, ``Maximum``, ``Count``|
|``DateTime``, ``DateTimeOffset``, ``DateOnly``, ``TimeOnly``, ``TimeSpan``|``Minimum``, ``Maximum``, ``Count``|
|anything else|``Count``|

``None`` is always available and removes the summary. ``Count`` counts rows, not values. A function set in XAML stays in the menu, even when it isn't suggested for that type.

```xml
<!-- offer your own list for a column -->
<DataGridTextColumn mvvm:DataGridSummaryBar.Functions="Sum, Maximum"
                    Binding="{Binding Price}"
                    Header="Price" />

<!-- lock a column -->
<DataGridTextColumn mvvm:DataGridSummaryBar.Function="Count"
                    mvvm:DataGridSummaryBar.Functions="None"
                    Binding="{Binding Id}"
                    Header="Id" />

<!-- turn the menu off for the whole bar -->
<mvvm:DataGridSummaryBar CanChangeFunction="False" DataGrid="{Binding ElementName=Grid}" />
```

#### Saving the layout

Bind ``DataGridLayoutBehavior.Layout`` to a property of your viewmodel. The behavior replaces its value when the user changes a column, and applies a layout you assign. The toolkit doesn't write anything to disk, so saving it is up to the application:

```cs
public sealed partial class OrdersViewModel : ViewModelBase
{
    [ObservableProperty]
    public partial DataGridLayout? Layout { get; set; }
}
```

``DataGridLayout`` contains no WPF types, so any serializer works. Load it before the grid is shown and save it when the application exits:

```cs
// at startup
if (File.Exists(path))
{
    viewModel.Layout = JsonSerializer.Deserialize<DataGridLayout>(File.ReadAllText(path));
}

// on exit
File.WriteAllText(path, JsonSerializer.Serialize(viewModel.Layout));
```

Columns are matched by their property path, so give columns without a binding a ``SortMemberPath`` or a header. Columns missing from a stored layout keep their settings, and entries without a column are ignored. A layout stored without ``SummaryFunction`` leaves the functions of the columns as they are.

The WPF sample stores the layout of its grids with [Jot](https://github.com/anakic/Jot), see ``App.xaml.cs`` and ``NavigationViewModel.cs``.

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
|``AsyncImageLoadingBehavior``|``Image``|Load an image through [MvvmScarletToolkit.ImageLoading](https://github.com/Insire/MvvmScarletToolkit/blob/master/src/MvvmScarletToolkit.ImageLoading/readme.md), and report ``State``, ``IsPending``, ``IsLoading``, ``IsLoaded`` and ``IsFailed``.|
|``AutoRepositionPopupBehavior``|``Popup``|Keep a popup at its position, when its window is moved.|
|``AutoScrollBehavior``|``Selector``|Scroll to the end of e.g. a ``ListBox`` or ``DataGrid``. Pauses when the user scrolls manually.|
|``DataGridColumnChooserBehavior``|``DataGrid``|Hide and show columns through a context menu on their headers.|
|``DataGridFilterBehavior``|``DataGrid``|Connect a ``DataGridFilterViewModel`` to a grid.|
|``DataGridGroupingBehavior``|``DataGrid``|Group by dragging a column header onto ``DropTarget``.|
|``DataGridLayoutBehavior``|``DataGrid``|Keep a bindable ``DataGridLayout`` in sync with the columns.|
|``LaunchNavigateUriAsNewProcessBehavior``|``Hyperlink``|Open ``Hyperlink.NavigateUri`` in a new process.|
|``MultiListBoxSelectionBehavior``|``ListBox``|Bind ``ListBox.SelectedItems`` to an existing ``IList``.|
|``MultiSelectionBehavior``|``MultiSelector``|Bind ``SelectedItems`` of e.g. a ``DataGrid`` to an existing ``IList``.|
|``PasswordBindingBehavior``|``PasswordBox``|Bind the password through ``ClearTextPassword``.|
|``ScrollSelectionIntoViewBehavior``|``ItemsControl``|Scroll the selected item of a ``ListBox``, ``DataGrid`` or ``TreeView`` into view. A custom ``TreeViewItem`` template needs a ``PART_Header`` to scroll to the selected row only.|
|``ScrollToEndCommandBehavior``|``FrameworkElement``|Execute ``Command`` when a scrollviewer reaches its vertical or horizontal end, sampled at ``Interval``. Useful for loading the next page.|
|``SelectedTreeViewItemBehavior``|``TreeView``|Enable twoway binding of the selected item.|
|``SmoothProgressBehavior``|``ProgressBar``|Animate changes of ``Percentage``, instead of jumping to the new value.|
|``TextBlockSelectionBehavior``|``TextBlock``|Select and copy text. See [Selecting text](#selecting-text).|
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
|``TextBlockSelection.IsEnabled``|Attached alternative to ``TextBlockSelectionBehavior``, that can be set from a style. See [Selecting text](#selecting-text).|

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
|``IoCRegistrationExtensions``|Register a container in a ``ResourceDictionary`` with ``Register``, and resolve services from a ``FrameworkElement`` with ``GetService<T>``.|

## Contents

These classes implement the services used by [``IScarletCommandBuilder``](https://github.com/Insire/MvvmScarletToolkit/blob/master/src/MvvmScarletToolkit.Commands/readme.md). ``ScarletCommandBuilder.Default`` combines them.

|class|summary|
|---|---|
|[``ScarletCommandBuilder``](https://github.com/Insire/MvvmScarletToolkit/blob/master/src/MvvmScarletToolkit.Wpf/Implementations/ScarletCommandBuilder.cs)|Fluent builder for ConcurrentCommand instances.|
|[``ScarletCommandManager``](https://github.com/Insire/MvvmScarletToolkit/blob/master/src/MvvmScarletToolkit.Wpf/Implementations/ScarletCommandManager.cs)|Wrapper for [System.Windows.Input.CommandManager](https://docs.microsoft.com/en-gb/dotnet/api/system.windows.input.commandmanager) to support Dependency Injection.|
|[``ScarletDispatcher``](https://github.com/Insire/MvvmScarletToolkit/blob/master/src/MvvmScarletToolkit.Wpf/Implementations/ScarletDispatcher.cs)|Wrapper for [System.Windows.Threading.Dispatcher](https://docs.microsoft.com/en-us/dotnet/api/system.windows.threading.dispatcher) to support Dependency Injection and Task based ICommand implementations in this library.|
|[``ScarletExitService``](https://github.com/Insire/MvvmScarletToolkit/blob/master/src/MvvmScarletToolkit.Wpf/Implementations/ScarletExitService.cs)|In WPF ui controls dont always raise their Unloaded-event, when the application is being closed. ScarletExitService takes care of executing ``IVirtualizationViewModel.UnloadCommand`` when the application exits.|
|[``ScarletLocalizationProvider``](https://github.com/Insire/MvvmScarletToolkit/blob/master/src/MvvmScarletToolkit.Wpf/Implementations/ScarletLocalizationProvider.cs)|Dummy Implementation of ``ILocalizationProvider``, which returns keys unchanged and offers no languages.|
|[``ScarletWeakEventManager``](https://github.com/Insire/MvvmScarletToolkit/blob/master/src/MvvmScarletToolkit.Wpf/Implementations/ScarletWeakEventManager.cs)|Wrapper for [System.Windows.WeakEventManager](https://docs.microsoft.com/en-us/dotnet/api/system.windows.weakeventmanager) to support Dependency Injection.|
|[``WpfImageFactory``](https://github.com/Insire/MvvmScarletToolkit/blob/master/src/MvvmScarletToolkit.Wpf/Implementations/WpfImageFactory.cs)|``IImageFactory<BitmapSource>`` for [MvvmScarletToolkit.ImageLoading](https://github.com/Insire/MvvmScarletToolkit/blob/master/src/MvvmScarletToolkit.ImageLoading/readme.md). Decodes images directly to the requested size, keeping their aspect ratio. Saves them as JPEG with quality 90, or as PNG when they can contain transparency.|
