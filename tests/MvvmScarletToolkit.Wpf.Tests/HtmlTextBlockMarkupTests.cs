using MvvmScarletToolkit.Wpf.Tests.Util;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;

namespace MvvmScarletToolkit.Wpf.Tests
{
    public sealed class HtmlTextBlockMarkupTests : TraceTestBase
    {
        private static IEnumerable<Inline> Descendants(InlineCollection inlines)
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

        private static Run RunOf(TextBlock textBlock, string text) => Descendants(textBlock.Inlines).OfType<Run>().Single(p => p.Text == text);

        [Theory]
        [InlineData("a &lt;b&gt; c", "a <b> c")]
        [InlineData("List&lt;T&gt;", "List<T>")]
        [InlineData("R&amp;D", "R&D")]
        [InlineData("&quot;quoted&quot; &#39;single&#39;", "\"quoted\" 'single'")]
        [InlineData("a&nbsp;b", "a b")]
        [InlineData("&#x41;&#66;", "AB")]
        [InlineData("&unknown; stays", "&unknown; stays")]
        public Task Entities_Should_Be_Decoded(string html, string expected)
        {
            return StaRunner.Run(() =>
            {
                var textBlock = new HtmlTextBlock { Html = html };

                Assert.Equal(expected, HtmlTextBlockTests.TextOf(textBlock));

                return Task.CompletedTask;
            });
        }

        [Fact]
        public Task Encoded_Tags_Should_Not_Be_Formatted()
        {
            return StaRunner.Run(() =>
            {
                var textBlock = new HtmlTextBlock { Html = "&lt;b&gt;not bold&lt;/b&gt;" };

                Assert.Empty(HtmlTextBlockTests.BoldTextsOf(textBlock));

                return Task.CompletedTask;
            });
        }

        [Fact]
        public Task Entities_In_Attributes_Should_Be_Decoded()
        {
            return StaRunner.Run(() =>
            {
                var textBlock = new HtmlTextBlock { Html = "<a href=\"https://example.com/?a=1&amp;b=2\">link</a>" };

                var link = Descendants(textBlock.Inlines).OfType<Hyperlink>().Single();
                Assert.Equal(new Uri("https://example.com/?a=1&b=2"), link.NavigateUri);

                return Task.CompletedTask;
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
            return StaRunner.Run(() =>
            {
                var textBlock = new HtmlTextBlock { FontSize = 10, Html = $"<{heading}>Title</{heading}> text" };

                Assert.Multiple(() =>
                {
                    Assert.Equal(["Title"], HtmlTextBlockTests.BoldTextsOf(textBlock));
                    Assert.Equal(10 * scale, RunOf(textBlock, "Title").FontSize, 6);
                    Assert.Equal(10, RunOf(textBlock, " text").FontSize);
                });

                return Task.CompletedTask;
            });
        }

        [Theory]
        [InlineData("<code>x</code>")]
        [InlineData("<pre>x</pre>")]
        public Task Code_And_Pre_Should_Use_A_Monospace_Font(string html)
        {
            return StaRunner.Run(() =>
            {
                var textBlock = new HtmlTextBlock { Html = $"{html} y" };

                Assert.Multiple(() =>
                {
                    Assert.StartsWith("Consolas", RunOf(textBlock, "x").FontFamily.Source);
                    Assert.Equal(textBlock.FontFamily, RunOf(textBlock, " y").FontFamily);
                });

                return Task.CompletedTask;
            });
        }

        [Fact]
        public Task Pre_Should_Keep_Whitespace_And_Show_Encoded_Markup()
        {
            return StaRunner.Run(() =>
            {
                var textBlock = new HtmlTextBlock { Html = "<pre>&lt;Grid&gt;\n    &lt;Button /&gt;\n&lt;/Grid&gt;</pre>" };

                Assert.Equal("<Grid>\n    <Button />\n</Grid>", HtmlTextBlockTests.TextOf(textBlock));

                return Task.CompletedTask;
            });
        }

        [Fact]
        public Task Unordered_Lists_Should_Show_Bullets()
        {
            return StaRunner.Run(() =>
            {
                var textBlock = new HtmlTextBlock { Html = "<ul><li>a</li><br><li>b</li></ul>" };

                Assert.Equal("• a\r\n• b", HtmlTextBlockTests.TextOf(textBlock));

                return Task.CompletedTask;
            });
        }

        [Fact]
        public Task Ordered_Lists_Should_Be_Numbered()
        {
            return StaRunner.Run(() =>
            {
                var textBlock = new HtmlTextBlock { Html = "<ol><li>a</li><br><li>b</li></ol>" };

                Assert.Equal("1. a\r\n2. b", HtmlTextBlockTests.TextOf(textBlock));

                return Task.CompletedTask;
            });
        }

        [Fact]
        public Task Nested_Lists_Should_Be_Indented_And_Count_Separately()
        {
            return StaRunner.Run(() =>
            {
                var textBlock = new HtmlTextBlock { Html = "<ol><li>a</li><br><li>b<br><ul><li>c</li></ul></li><br><li>d</li></ol>" };

                Assert.Equal("1. a\r\n2. b\r\n    • c\r\n3. d", HtmlTextBlockTests.TextOf(textBlock));

                return Task.CompletedTask;
            });
        }

        [Fact]
        public Task List_Markers_Should_Follow_The_Surrounding_Format()
        {
            return StaRunner.Run(() =>
            {
                var textBlock = new HtmlTextBlock { Html = "<b><ul><li>a</li></ul></b>" };

                Assert.Equal(["• ", "a"], HtmlTextBlockTests.BoldTextsOf(textBlock));

                return Task.CompletedTask;
            });
        }

        [Fact]
        public Task An_Li_Outside_Of_A_List_Should_Show_A_Bullet()
        {
            return StaRunner.Run(() =>
            {
                var textBlock = new HtmlTextBlock { Html = "<li>a</li>" };

                Assert.Equal("• a", HtmlTextBlockTests.TextOf(textBlock));

                return Task.CompletedTask;
            });
        }

        [Theory]
        [InlineData("R&amp;D and r&amp;d", "R&D", "R&D and r&d", 2)]
        [InlineData("a &amp; b", "a", "a & b", 1)]
        [InlineData("&lt;T&gt;", "T", "<T>", 1)]
        public Task Highlight_Should_Match_Decoded_Text(string html, string highlight, string expected, int count)
        {
            return StaRunner.Run(() =>
            {
                var textBlock = new HtmlHighlightTextBlock { Highlight = highlight, Html = html };

                Assert.Multiple(() =>
                {
                    Assert.Equal(expected, HtmlTextBlockTests.TextOf(textBlock));
                    Assert.Equal(count, HtmlTextBlockTests.BoldTextsOf(textBlock).Length);
                });

                return Task.CompletedTask;
            });
        }

        [Fact]
        public Task Links_Should_Not_Get_A_Command_By_Default()
        {
            return StaRunner.Run(() =>
            {
                var textBlock = new HtmlTextBlock { Html = "<a href='https://example.com'>link</a>" };

                Assert.Null(Descendants(textBlock.Inlines).OfType<Hyperlink>().Single().Command);

                return Task.CompletedTask;
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
            return StaRunner.Run(() =>
            {
                var textBlock = new HtmlTextBlock { OpenLinksInBrowser = true, Html = $"<a href='{href}'>link</a>" };

                var link = Descendants(textBlock.Inlines).OfType<Hyperlink>().Single();

                Assert.Multiple(() =>
                {
                    Assert.Same(HtmlTag.OpenLinkCommand, link.Command);
                    Assert.Equal(link.NavigateUri, link.CommandParameter);
                    Assert.Equal(canOpen, link.Command.CanExecute(link.CommandParameter));
                });

                return Task.CompletedTask;
            });
        }

        [Fact]
        public Task OpenLinksInBrowser_Should_Apply_To_Image_Links()
        {
            return StaRunner.Run(() =>
            {
                var textBlock = new HtmlTextBlock { OpenLinksInBrowser = true, Html = "<img alt='x' href='https://example.com/'>" };

                Assert.Same(HtmlTag.OpenLinkCommand, Assert.IsType<Hyperlink>(Assert.Single(textBlock.Inlines)).Command);

                return Task.CompletedTask;
            });
        }

        [Fact]
        public Task Changing_OpenLinksInBrowser_Should_Rebuild_The_Links()
        {
            return StaRunner.Run(() =>
            {
                var textBlock = new HtmlTextBlock { Html = "<a href='https://example.com'>link</a>" };

                textBlock.OpenLinksInBrowser = true;

                Assert.NotNull(Descendants(textBlock.Inlines).OfType<Hyperlink>().Single().Command);

                return Task.CompletedTask;
            });
        }
    }
}
