# HtmlTextBlock parser

UI-independent part of `HtmlTextBlock`, compiled into both `MvvmScarletToolkit.Wpf` and `MvvmScarletToolkit.Avalonia` as linked source files. Everything here is `internal` and must not reference WPF or Avalonia types.

- `HtmlTagTree` builds the tag tree, `HtmlTag` holds a tag or text with its attributes, read by `HtmlAttributes`.
- `InlineCreationContext<TColor>` tracks the format at the current position, the UI package passes its color parser.
- `InlineStyle` reads `style` attributes, `HtmlHighlighter` prepares the html of `HtmlHighlightTextBlock`.

Each UI package keeps its own control, inline creation and image loading.
