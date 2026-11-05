using Avalonia.Controls;
using Avalonia.Controls.Documents;
using Avalonia.Media;
using MvvmScarletToolkit.Avalonia.Tests.Util;
using System.Globalization;

namespace MvvmScarletToolkit.Avalonia.Tests
{
    public sealed class HtmlTextBlockTests : TraceTestBase
    {
        private readonly AvaloniaPlatformFixture _platform;

        public HtmlTextBlockTests(AvaloniaPlatformFixture platform)
        {
            _platform = platform;
        }

        internal static string TextOf(TextBlock textBlock) => textBlock.Inlines!.Text ?? string.Empty;

        internal static IEnumerable<Inline> Descendants(InlineCollection inlines)
        {
            foreach (var inline in inlines)
            {
                yield return inline;

                if (inline is Span span)
                {
                    foreach (var child in Descendants(span.Inlines))
                    {
                        yield return child;
                    }
                }
            }
        }

        internal static string[] BoldTextsOf(TextBlock textBlock)
        {
            return Descendants(textBlock.Inlines!)
                .OfType<Run>()
                .Where(p => p.FontWeight == FontWeight.Bold)
                .Select(p => p.Text ?? string.Empty)
                .ToArray();
        }

        internal static Run RunOf(TextBlock textBlock, string text) => Descendants(textBlock.Inlines!).OfType<Run>().Single(p => p.Text == text);

        internal static HyperlinkButton[] LinksOf(TextBlock textBlock)
        {
            return Descendants(textBlock.Inlines!)
                .OfType<InlineUIContainer>()
                .Select(p => p.Child)
                .OfType<HyperlinkButton>()
                .ToArray();
        }

        private Task Run(Action test)
        {
            return _platform.RunOnUIThread(() =>
            {
                test();

                return Task.CompletedTask;
            });
        }

        [Fact]
        public Task Should_Render_Text_And_Formatting()
        {
            return Run(() =>
            {
                var textBlock = new HtmlTextBlock { Html = "plain <b>bold</b> <i>italic</i> <u>underlined</u>" };

                Assert.Multiple(() =>
                {
                    Assert.Equal("plain bold italic underlined", TextOf(textBlock));
                    Assert.Equal(["bold"], BoldTextsOf(textBlock));
                    Assert.Equal(FontStyle.Italic, RunOf(textBlock, "italic").FontStyle);
                    Assert.Same(TextDecorations.Underline, RunOf(textBlock, "underlined").TextDecorations);
                });
            });
        }

        [Theory]
        [InlineData("x <!-- a > b --> y", "x  y")]
        [InlineData("x <!-- > --> y", "x  y")]
        [InlineData("x <!-- comment --> y", "x  y")]
        [InlineData("a<!-- one -->b<b>c</b><!-- two -->d", "abcd")]
        [InlineData("a <b>b</b> c", "a b c")]
        [InlineData("<b>a</b>", "a")]
        [InlineData("a < b", "a < b")]
        [InlineData("a <> b", "a  b")]
        [InlineData("a < b> c", "a  c")]
        [InlineData("<unknown>a</unknown>", "a")]
        [InlineData("<!-- unterminated", "<!-- unterminated")]
        [InlineData("a <!-- x -- y --> b", "a  b")]
        public Task Should_Scan_Edge_Cases(string html, string expected)
        {
            return Run(() =>
            {
                var textBlock = new HtmlTextBlock { Html = html };

                Assert.Equal(expected, TextOf(textBlock));
            });
        }

        [Fact]
        public Task Setting_Null_Should_Clear_The_Text()
        {
            return Run(() =>
            {
                var textBlock = new HtmlTextBlock { Html = "text" };

                textBlock.Html = null;

                Assert.Empty(textBlock.Inlines!);
            });
        }

        [Fact]
        public Task Text_After_The_Last_Tag_Should_Not_Repeat_That_Tag()
        {
            return Run(() =>
            {
                var textBlock = new HtmlTextBlock { Html = "a<br>b" };

                Assert.Single(textBlock.Inlines!.OfType<LineBreak>());
            });
        }

        [Theory]
        [InlineData("en-US")]
        [InlineData("tr-TR")]
        public Task Uppercase_Tags_Should_Be_Recognized_Independent_Of_Culture(string cultureName)
        {
            return Run(() =>
            {
                var culture = CultureInfo.CurrentCulture;
                try
                {
                    // in turkish "I".ToLower() is "ı", so <I> would not be recognized as italic
                    CultureInfo.CurrentCulture = new CultureInfo(cultureName);

                    var textBlock = new HtmlTextBlock { Html = "<I>italic</I> <B>bold</B>" };

                    Assert.Multiple(() =>
                    {
                        Assert.Equal(FontStyle.Italic, RunOf(textBlock, "italic").FontStyle);
                        Assert.Equal(["bold"], BoldTextsOf(textBlock));
                    });
                }
                finally
                {
                    CultureInfo.CurrentCulture = culture;
                }
            });
        }

        [Fact]
        public Task Should_Parse_Large_Html_In_Linear_Time()
        {
            return Run(() =>
            {
                var html = string.Concat(Enumerable.Repeat("text <b>bold</b> and <i>italic</i> ", 5_000));

                var watch = System.Diagnostics.Stopwatch.StartNew();
                var textBlock = new HtmlTextBlock { Html = html };
                watch.Stop();

                Assert.Equal(5_000, BoldTextsOf(textBlock).Length);
                Assert.True(watch.Elapsed < TimeSpan.FromSeconds(3), $"parsing took {watch.Elapsed}");
            });
        }

        [Theory]
        [InlineData("a &lt;b&gt; c", "a <b> c")]
        [InlineData("R&amp;D", "R&D")]
        [InlineData("&quot;quoted&quot; &#39;single&#39;", "\"quoted\" 'single'")]
        [InlineData("a&nbsp;b", "a\u00A0b")]
        [InlineData("&#x41;&#66;", "AB")]
        [InlineData("&unknown; stays", "&unknown; stays")]
        public Task Entities_Should_Be_Decoded(string html, string expected)
        {
            return Run(() =>
            {
                var textBlock = new HtmlTextBlock { Html = html };

                Assert.Equal(expected, TextOf(textBlock));
            });
        }

        [Fact]
        public Task Encoded_Tags_Should_Not_Be_Formatted()
        {
            return Run(() =>
            {
                var textBlock = new HtmlTextBlock { Html = "&lt;b&gt;not bold&lt;/b&gt;" };

                Assert.Empty(BoldTextsOf(textBlock));
            });
        }

        [Theory]
        [InlineData("h1", 2.0)]
        [InlineData("h2", 1.5)]
        [InlineData("h3", 1.25)]
        [InlineData("h4", 1.1)]
        [InlineData("h5", 1.0)]
        [InlineData("h6", 0.9)]
        public Task Headings_Should_Be_Bold_And_Scaled(string heading, double scale)
        {
            return Run(() =>
            {
                var textBlock = new HtmlTextBlock { FontSize = 10, Html = $"<{heading}>Title</{heading}> text" };

                Assert.Multiple(() =>
                {
                    Assert.Equal(["Title"], BoldTextsOf(textBlock));
                    Assert.Equal(10 * scale, RunOf(textBlock, "Title").FontSize, 6);
                    Assert.False(RunOf(textBlock, " text").IsSet(TextElement.FontSizeProperty));
                });
            });
        }

        [Fact]
        public Task Changing_The_FontSize_Should_Scale_Headings_Again()
        {
            return Run(() =>
            {
                // styles usually set the font size after the html
                var textBlock = new HtmlTextBlock { Html = "<h1>Title</h1>" };

                textBlock.FontSize = 20;

                Assert.Equal(40, RunOf(textBlock, "Title").FontSize, 6);
            });
        }

        [Theory]
        [InlineData("sub", BaselineAlignment.Subscript)]
        [InlineData("sup", BaselineAlignment.Superscript)]
        public Task Sub_And_Sup_Should_Be_Smaller_And_Shifted(string tag, BaselineAlignment alignment)
        {
            return Run(() =>
            {
                var textBlock = new HtmlTextBlock { FontSize = 10, Html = $"H<{tag}>2</{tag}>O" };

                Assert.Multiple(() =>
                {
                    Assert.Equal(alignment, RunOf(textBlock, "2").BaselineAlignment);
                    Assert.True(RunOf(textBlock, "2").FontSize < 10);
                });
            });
        }

        [Theory]
        [InlineData("<code>x</code>")]
        [InlineData("<pre>x</pre>")]
        public Task Code_And_Pre_Should_Use_A_Monospace_Font(string html)
        {
            return Run(() =>
            {
                var textBlock = new HtmlTextBlock { Html = $"{html} y" };

                Assert.Multiple(() =>
                {
                    Assert.Same(HtmlTextBlock.Monospace, RunOf(textBlock, "x").FontFamily);
                    Assert.False(RunOf(textBlock, " y").IsSet(TextElement.FontFamilyProperty));
                });
            });
        }

        [Fact]
        public Task Pre_Should_Keep_Whitespace_And_Show_Encoded_Markup()
        {
            return Run(() =>
            {
                var textBlock = new HtmlTextBlock { Html = "<pre>&lt;Grid&gt;\n    &lt;Button /&gt;\n&lt;/Grid&gt;</pre>" };

                Assert.Equal("<Grid>\n    <Button />\n</Grid>", TextOf(textBlock));
            });
        }

        [Fact]
        public Task Unordered_Lists_Should_Show_Bullets()
        {
            return Run(() =>
            {
                var textBlock = new HtmlTextBlock { Html = "<ul><li>a</li><br><li>b</li></ul>" };

                Assert.Equal($"• a{Environment.NewLine}• b", TextOf(textBlock));
            });
        }

        [Fact]
        public Task Nested_Lists_Should_Be_Indented_And_Count_Separately()
        {
            return Run(() =>
            {
                var textBlock = new HtmlTextBlock { Html = "<ol><li>a</li><br><li>b<br><ul><li>c</li></ul></li><br><li>d</li></ol>" };
                var nl = Environment.NewLine;

                Assert.Equal($"1. a{nl}2. b{nl}    • c{nl}3. d", TextOf(textBlock));
            });
        }

        [Fact]
        public Task List_Markers_Should_Follow_The_Surrounding_Format()
        {
            return Run(() =>
            {
                var textBlock = new HtmlTextBlock { Html = "<b><ul><li>a</li></ul></b>" };

                Assert.Equal(["• ", "a"], BoldTextsOf(textBlock));
            });
        }

        [Theory]
        [InlineData("<font color=\"red\">x</font>")]
        [InlineData("<font color='#FF0000'>x</font>")]
        [InlineData("<span style=\"color: #f00\">x</span>")]
        [InlineData("<color color=\"Red\">x</color>")]
        public Task Text_Color_Should_Be_Applied(string html)
        {
            return Run(() =>
            {
                var textBlock = new HtmlTextBlock { Html = html };

                Assert.Equal(Colors.Red, Assert.IsType<SolidColorBrush>(RunOf(textBlock, "x").Foreground).Color);
            });
        }

        [Fact]
        public Task Innermost_Color_Should_Win_And_Invalid_Colors_Should_Be_Ignored()
        {
            return Run(() =>
            {
                var textBlock = new HtmlTextBlock { Html = "<font color='red'>a<font color='blue'>b<font color='nope'>c</font></font>d</font>" };

                Assert.Multiple(() =>
                {
                    Assert.Equal(Colors.Red, Assert.IsType<SolidColorBrush>(RunOf(textBlock, "a").Foreground).Color);
                    Assert.Equal(Colors.Blue, Assert.IsType<SolidColorBrush>(RunOf(textBlock, "b").Foreground).Color);
                    Assert.Equal(Colors.Blue, Assert.IsType<SolidColorBrush>(RunOf(textBlock, "c").Foreground).Color);
                    Assert.Equal(Colors.Red, Assert.IsType<SolidColorBrush>(RunOf(textBlock, "d").Foreground).Color);
                });
            });
        }

        [Fact]
        public Task Binding_Tags_Should_Read_The_DataContext()
        {
            return Run(() =>
            {
                var textBlock = new HtmlTextBlock { Html = "Hello <binding path='Name'>" };
                Assert.Equal("Hello {Binding}", TextOf(textBlock));

                textBlock.DataContext = new { Name = "World" };

                Assert.Equal("Hello World", TextOf(textBlock));
            });
        }

        [Fact]
        public Task Links_Should_Be_Hyperlink_Buttons_Without_A_Command_By_Default()
        {
            return Run(() =>
            {
                var textBlock = new HtmlTextBlock { Html = "<a href=\"https://example.com/?a=1&amp;b=2\">link</a>" };

                var link = Assert.Single(LinksOf(textBlock));

                Assert.Multiple(() =>
                {
                    Assert.Null(link.Command);
                    Assert.Null(link.NavigateUri);
                    Assert.Equal("https://example.com/?a=1&b=2", ToolTip.GetTip(link));
                    Assert.Equal("link", TextOf(Assert.IsType<TextBlock>(link.Content)));
                });
            });
        }

        [Theory]
        [InlineData("https://example.com/", true)]
        [InlineData("http://example.com/", true)]
        [InlineData("mailto:someone@example.com", true)]
        [InlineData("file:///C:/Windows/System32/calc.exe", false)]
        [InlineData("relative/page.html", false)]
        public Task OpenLinksInBrowser_Should_Only_Allow_Web_And_Mail_Links(string href, bool canOpen)
        {
            return Run(() =>
            {
                var textBlock = new HtmlTextBlock { OpenLinksInBrowser = true, Html = $"<a href='{href}'>link</a>" };

                var link = Assert.Single(LinksOf(textBlock));

                Assert.Multiple(() =>
                {
                    Assert.Same(textBlock.LinkCommand, link.Command);
                    Assert.Equal(canOpen, link.Command!.CanExecute(link.CommandParameter));
                });
            });
        }

        [Fact]
        public Task Changing_OpenLinksInBrowser_Should_Rebuild_The_Links()
        {
            return Run(() =>
            {
                var textBlock = new HtmlTextBlock { Html = "<a href='https://example.com'>link</a>" };

                textBlock.OpenLinksInBrowser = true;

                Assert.NotNull(Assert.Single(LinksOf(textBlock)).Command);
            });
        }

        [Theory]
        [InlineData("https://example.com/docs/", "page.html", "https://example.com/docs/page.html")]
        [InlineData("https://example.com/docs/index.html", "../about.html", "https://example.com/about.html")]
        [InlineData(null, "page.html", "page.html")]
        public Task Relative_Links_Should_Be_Resolved_Against_The_BaseUri(string? baseUri, string href, string expected)
        {
            return Run(() =>
            {
                var textBlock = new HtmlTextBlock
                {
                    BaseUri = baseUri is null ? null : new Uri(baseUri),
                    OpenLinksInBrowser = true,
                    Html = $"<a href='{href}'>link</a>",
                };

                var link = Assert.Single(LinksOf(textBlock));

                Assert.Multiple(() =>
                {
                    Assert.Equal(expected, ToolTip.GetTip(link));
                    Assert.Equal(expected, Assert.IsType<Uri>(link.CommandParameter).OriginalString);
                });
            });
        }

        [Fact]
        public Task Text_In_Links_Should_Keep_Its_Format()
        {
            return Run(() =>
            {
                var textBlock = new HtmlTextBlock { Html = "<a href='https://example.com'><b>bold</b> link</a>" };

                var links = LinksOf(textBlock);

                Assert.Multiple(() =>
                {
                    Assert.Equal(2, links.Length);
                    Assert.Equal(["bold"], BoldTextsOf(Assert.IsType<TextBlock>(links[0].Content)));
                });
            });
        }
    }

    public sealed class HtmlHighlightTextBlockTests : TraceTestBase
    {
        private readonly AvaloniaPlatformFixture _platform;

        public HtmlHighlightTextBlockTests(AvaloniaPlatformFixture platform)
        {
            _platform = platform;
        }

        private Task Run(Action test)
        {
            return _platform.RunOnUIThread(() =>
            {
                test();

                return Task.CompletedTask;
            });
        }

        [Theory]
        [InlineData("hello world", "WORLD", "hello world", new[] { "world" })]
        [InlineData("banana", "a", "banana", new[] { "a", "a", "a" })]
        [InlineData("<i>abc</i>", "b", "abc", new[] { "b" })]
        [InlineData("abc a.c", "a.c", "abc a.c", new[] { "a.c" })]
        [InlineData("R&amp;D and r&amp;d", "R&D", "R&D and r&d", new[] { "R&D", "r&d" })]
        [InlineData("&lt;T&gt;", "T", "<T>", new[] { "T" })]
        public Task Should_Highlight_Matches_In_Bold(string html, string highlight, string expected, string[] bold)
        {
            return Run(() =>
            {
                var textBlock = new HtmlHighlightTextBlock { Highlight = highlight, Html = html };

                Assert.Multiple(() =>
                {
                    Assert.Equal(expected, HtmlTextBlockTests.TextOf(textBlock));
                    Assert.Equal(bold, HtmlTextBlockTests.BoldTextsOf(textBlock));
                });
            });
        }

        [Fact]
        public Task Changing_Highlight_Should_Render_Again()
        {
            return Run(() =>
            {
                var textBlock = new HtmlHighlightTextBlock { Html = "abc" };
                Assert.Empty(HtmlTextBlockTests.BoldTextsOf(textBlock));

                textBlock.Highlight = "b";

                Assert.Equal(["b"], HtmlTextBlockTests.BoldTextsOf(textBlock));
            });
        }

        [Fact]
        public Task Should_Tolerate_Null_Html()
        {
            return Run(() =>
            {
                var textBlock = new HtmlHighlightTextBlock { Highlight = "b", Html = "abc" };

                textBlock.Html = null;

                Assert.Empty(textBlock.Inlines!);
            });
        }
    }
}
