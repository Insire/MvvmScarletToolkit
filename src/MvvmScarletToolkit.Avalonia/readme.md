![MvvmScarletToolkit.Avalonia](https://raw.githubusercontent.com/Insire/MvvmScarletToolkit/master/assets/branding/nuget/MvvmScarletToolkit.Avalonia.png)

## Goals

This library aims to provide implementations for avalonia specific classes used by the framework.

## Features

- **Toolkit services for Avalonia:** the dispatcher, command manager, exit service and weak event manager, which `ScarletCommandBuilder.Default` combines. Viewmodels and commands work the same as on WPF.
- **Unload on exit:** business viewmodels are unloaded when a classic desktop application shuts down. Other application lifetimes are not supported by `ScarletExitService`.
- **Async image loading:** `AsyncImageLoadingBehavior` loads images through [MvvmScarletToolkit.ImageLoading](https://github.com/Insire/MvvmScarletToolkit/blob/master/src/MvvmScarletToolkit.ImageLoading/readme.md).
- **Converters as markup extensions:** `ConverterMarkupExtension<T>` is a base class for writing converters that don't have to be declared as resources.
- **Virtualizing panels:** `VirtualizingTilePanel` and `VirtualizingWrapPanel` keep lists with many items per row fast.
- **Html text:** `HtmlTextBlock` and `HtmlHighlightTextBlock` display formatted text, links and images from a small subset of html.

Apart from these panels and `HtmlTextBlock`, the behaviors, converters and controls of [MvvmScarletToolkit.Wpf](https://github.com/Insire/MvvmScarletToolkit/blob/master/src/MvvmScarletToolkit.Wpf/readme.md) are not available for Avalonia. Text selection doesn't need the toolkit here, as Avalonia comes with a `SelectableTextBlock`.

## Contents

|class|summary|
|---|---|
|[``ScarletCommandBuilder``](https://github.com/Insire/MvvmScarletToolkit/blob/master/src/MvvmScarletToolkit.Avalonia/Implementations/ScarletCommandBuilder.cs)|Fluent builder for ConcurrentCommand instances.|
|[``ScarletCommandManager``](https://github.com/Insire/MvvmScarletToolkit/blob/master/src/MvvmScarletToolkit.Avalonia/Implementations/ScarletCommandManager.cs)|Raises ``RequerySuggested`` on the UI thread after user input and when requested from any thread, so that commands can update their ``CanExecute`` state.|
|[``ScarletDispatcher``](https://github.com/Insire/MvvmScarletToolkit/blob/master/src/MvvmScarletToolkit.Avalonia/Implementations/ScarletDispatcher.cs)|Wrapper for the Avalonia dispatcher to support Dependency Injection and Task based ICommand implementations in this library.|
|[``ScarletExitService``](https://github.com/Insire/MvvmScarletToolkit/blob/master/src/MvvmScarletToolkit.Avalonia/Implementations/ScarletExitService.cs)|Takes care of executing ``IVirtualizationViewModel.UnloadCommand`` when the application exits.|
|[``ScarletLocalizationProvider``](https://github.com/Insire/MvvmScarletToolkit/blob/master/src/MvvmScarletToolkit.Avalonia/Implementations/ScarletLocalizationProvider.cs)|Dummy Implementation of ``ILocalizationProvider``.|
|[``ScarletWeakEventManager``](https://github.com/Insire/MvvmScarletToolkit/blob/master/src/MvvmScarletToolkit.Avalonia/Implementations/ScarletWeakEventManager.cs)|Weak event subscriptions for ``INotifyPropertyChanged``, to support Dependency Injection.|
|[``AvaloniaImageFactory``](https://github.com/Insire/MvvmScarletToolkit/blob/master/src/MvvmScarletToolkit.Avalonia/Implementations/AvaloniaImageFactory.cs)|``IImageFactory<Bitmap>`` for [MvvmScarletToolkit.ImageLoading](https://github.com/Insire/MvvmScarletToolkit/blob/master/src/MvvmScarletToolkit.ImageLoading/readme.md). Decodes images directly to the requested size, keeping their aspect ratio. Saves them as JPEG with quality 90, or as PNG when their source can contain transparency. Reads image headers with SkiaSharp, so the native Skia library has to be available, which ``Avalonia.Skia`` provides.|
|[``AsyncImageLoadingBehavior``](https://github.com/Insire/MvvmScarletToolkit/blob/master/src/MvvmScarletToolkit.Avalonia/Behaviors/AsyncImageLoadingBehavior.cs)|Attached properties for loading the source of an ``Image`` asynchronously.|
|[``ConverterMarkupExtension<T>``](https://github.com/Insire/MvvmScarletToolkit/blob/master/src/MvvmScarletToolkit.Avalonia/ConverterMarkupExtension.cs)|Base class for writing your own converters as markup extensions.|
|[``VirtualizingTilePanel``](https://github.com/Insire/MvvmScarletToolkit/blob/master/src/MvvmScarletToolkit.Avalonia/Controls/VirtualizingTilePanel.cs)|Virtualizing panel that lays out items as square tiles of ``ChildSize``.|
|[``VirtualizingWrapPanel``](https://github.com/Insire/MvvmScarletToolkit/blob/master/src/MvvmScarletToolkit.Avalonia/Controls/VirtualizingWrapPanel.cs)|Virtualizing panel that wraps items of different sizes into rows or columns. ``ItemWidth`` and ``ItemHeight`` set a fixed size, without one the sizes of items that were not realized yet are estimated.|
|[``VirtualizingItemsPanel``](https://github.com/Insire/MvvmScarletToolkit/blob/master/src/MvvmScarletToolkit.Avalonia/Controls/VirtualizingItemsPanel.cs)|Base class of both panels, it realizes and recycles containers. Derive from it to add another layout.|
|[``HtmlTextBlock``](https://github.com/Insire/MvvmScarletToolkit/blob/master/src/MvvmScarletToolkit.Avalonia/Controls/HtmlTextBlock/HtmlTextBlock.cs)|``TextBlock`` that displays formatted text, links and images from its ``Html`` property. See [HtmlTextBlock](#htmltextblock).|
|[``HtmlHighlightTextBlock``](https://github.com/Insire/MvvmScarletToolkit/blob/master/src/MvvmScarletToolkit.Avalonia/Controls/HtmlTextBlock/HtmlHighlightTextBlock.cs)|``HtmlTextBlock`` that also shows the text from its ``Highlight`` property in bold, e.g. for search results.|

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

Assign the image service once at startup, as described in [MvvmScarletToolkit.ImageLoading](https://github.com/Insire/MvvmScarletToolkit/blob/master/src/MvvmScarletToolkit.ImageLoading/readme.md), then set the attached properties on an `Image`:

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

### Virtualizing panels

Both panels realize only what is visible and rely on the `ScrollViewer` of the list, like the `VirtualizingStackPanel`. The direction items wrap in must not scroll, so disable the horizontal scrollbar for the default orientation:

```xml
<ListBox ItemsSource="{Binding Items}" ScrollViewer.HorizontalScrollBarVisibility="Disabled">
    <ListBox.ItemsPanel>
        <ItemsPanelTemplate>
            <mvvm:VirtualizingWrapPanel />
        </ItemsPanelTemplate>
    </ListBox.ItemsPanel>
</ListBox>
```

`VirtualizingWrapPanel` wraps into columns with `Orientation="Vertical"`. Items may differ in size, so the sizes of items that were not realized yet are estimated, and the scrollbar can shift a little while scrolling. Set `ItemWidth` and `ItemHeight` to avoid that.

`VirtualizingTilePanel` fits as many columns of `ChildSize` tiles as the width allows:

```xml
<ItemsPanelTemplate>
    <mvvm:VirtualizingTilePanel ChildSize="50" />
</ItemsPanelTemplate>
```

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

### HtmlTextBlock

`HtmlTextBlock` supports the same small subset of html as the [WPF version](https://github.com/Insire/MvvmScarletToolkit/blob/master/src/MvvmScarletToolkit.Wpf/readme.md#htmltextblock). Attribute values can be double quoted, single quoted or unquoted, and entities like `&lt;`, `&amp;` and `&#39;` are decoded.

Unlike a browser, `HtmlTextBlock` keeps whitespace and line breaks of the text as they are, and block tags like headings and list items don't start a new line on their own. Put them on their own line in the html, or use `br`.

```xml
<mvvm:HtmlTextBlock Html="{Binding Html}" OpenLinksInBrowser="True" TextWrapping="Wrap" />

<mvvm:HtmlHighlightTextBlock Highlight="{Binding SearchText}" Html="{Binding Description}" />
```

|tag|effect|
|---|---|
|`b`, `i`, `u`, `sub`, `sup`|Bold, italic, underlined, subscript and superscript text. Subscript and superscript are shown smaller.|
|`h1` to `h6`|Bold text, scaled from 2 to 0.9 times the `FontSize` of the control. They follow when the font size changes, e.g. through a style.|
|`code`, `pre`|Monospace text.|
|`ul`, `ol`, `li`|Bullets or numbers in front of each `li`, nested lists are indented.|
|`a href="…"`|Link. Set `OpenLinksInBrowser="True"` to open clicked http, https and mailto links through the launcher of the `TopLevel`. Other links are disabled then, since a `file:` link could start any program.|
|`br`|Line break.|
|`font color="…"`, `color color="…"`, `span style="color: …"`|Text color: named colors, `#rgb`, `#rrggbb` or `#aarrggbb`. The innermost color wins, invalid colors are ignored.|
|`img`|Inline image with `src`, `width`, `height`, `alt`, `title`, `href` and a `style` with `width`, `height`, `margin`, `vertical-align` and `opacity`.|
|`binding path="…"`|Text of the named public property of the `DataContext`, read through reflection. Shows `{Binding}` if there is no such property.|

Avalonia has no hyperlink inline, so each link is a `HyperlinkButton` inside an `InlineUIContainer`. A link therefore doesn't wrap within itself, it moves to the next line as a whole.

`BaseUri` resolves relative image sources and links, like the address of a page in a browser. Set it to the uri of the html file itself, e.g. `new Uri(@"C:\docs\readme.html")`, and `<img src="images/logo.png">` loads `C:\docs\images\logo.png`. A folder needs a trailing slash.

Images with http(s), file and relative sources load through an `IImageService<Bitmap>`, so they are cached and load asynchronously once the control is shown. `HtmlTextBlock.ImageService` sets the service for one control, otherwise `AsyncImageLoadingBehavior.Loader` is used. When both width and height are known in px, the image is decoded at that size. Without a service, files and `avares://` assets are loaded directly, and web images show their `alt` text.

> Html from untrusted sources can make the control download images from any web address.

`HtmlTextBlock` derives from `TextBlock`, so its text can't be selected.

In trimmed or Native AOT apps, the trimmer can't see which properties a `binding` tag reads. Keep them, e.g. with `[DynamicDependency(DynamicallyAccessedMemberTypes.PublicProperties, typeof(MyViewModel))]` on the code that creates the viewmodel, otherwise the tag shows `{Binding}`.
