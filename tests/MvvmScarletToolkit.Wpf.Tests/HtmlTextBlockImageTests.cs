using MvvmScarletToolkit.Wpf.Tests.Util;
using System.Globalization;
using System.IO;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace MvvmScarletToolkit.Wpf.Tests
{
    // ImageLoader.AsyncImageLoader is static, tests that depend on it must not run in parallel with other users of it
    [Collection(ImageLoaderCollection)]
    public sealed class HtmlTextBlockImageTests : TraceTestBase, IDisposable
    {
        public const string ImageLoaderCollection = "ImageLoader.AsyncImageLoader";

        private static readonly Uri WebImage = new("https://example.com/image.png");

        private readonly IImageService<BitmapSource> _imageService = Substitute.For<IImageService<BitmapSource>>();
        private readonly string _directory;

        public HtmlTextBlockImageTests()
        {
            ImageLoader.AsyncImageLoader = null;

            _imageService.ProvideImageAsync(Arg.Any<Uri?>(), Arg.Any<ImageSize?>(), Arg.Any<Func<bool, Task>>(), Arg.Any<CancellationToken>())
                .Returns(_ => Task.FromResult<BitmapSource?>(TestBitmaps.Create(10, 10)));

            _directory = Path.Combine(Path.GetTempPath(), nameof(HtmlTextBlockImageTests), Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(_directory);
        }

        public void Dispose()
        {
            ImageLoader.AsyncImageLoader = null;
            Directory.Delete(_directory, true);
        }

        private Lazy<IImageService<BitmapSource>> Service => new(() => _imageService);

        private string CreatePngFile()
        {
            var path = Path.Combine(_directory, "image.png");
            using var png = TestBitmaps.CreatePng(4, 4);
            File.WriteAllBytes(path, png.ToArray());

            return path;
        }

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

        private static InlineUIContainer ContainerOf(TextBlock textBlock) => Descendants(textBlock.Inlines).OfType<InlineUIContainer>().Single();

        private static Image ImageOf(TextBlock textBlock) => Assert.IsType<Image>(ContainerOf(textBlock).Child);

        private static Color ColorOf(TextBlock textBlock, string text)
        {
            var run = Descendants(textBlock.Inlines).OfType<Run>().Single(p => p.Text == text);

            return Assert.IsType<SolidColorBrush>(run.Foreground).Color;
        }

        private static Window Show(FrameworkElement content)
        {
            var window = new Window
            {
                Content = content,
                Width = 200,
                Height = 100,
                Left = -10000,
                Top = -10000,
                WindowStyle = WindowStyle.None,
                ShowInTaskbar = false,
                ShowActivated = false,
            };
            window.Show();

            return window;
        }

        private static async Task WaitUntil(Func<bool> condition)
        {
            var timeout = DateTime.UtcNow.AddSeconds(5);
            while (!condition())
            {
                if (DateTime.UtcNow > timeout)
                {
                    throw new TimeoutException();
                }

                await Dispatcher.Yield(DispatcherPriority.ApplicationIdle);
                await Task.Delay(5);
            }
        }

        [Theory]
        [InlineData("a=\"x y\" b='z w' c=v d", "x y", "z w", "v", "TRUE")]
        [InlineData("A = \"x y\"   B='z w'  C=v D", "x y", "z w", "v", "TRUE")]
        public void Attributes_Should_Support_Quoted_Unquoted_And_Valueless(string attributes, string a, string b, string c, string d)
        {
            var result = new ParamParser(new HtmlAttributeStringSerializer()).StringToDictionary(attributes);

            Assert.Equal(new Dictionary<string, string> { ["a"] = a, ["b"] = b, ["c"] = c, ["d"] = d }, result);
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData("=")]
        [InlineData("= = x")]
        [InlineData("a=\"unterminated")]
        public void Attributes_Should_Not_Hang_On_Malformed_Input(string attributes)
        {
            _ = new ParamParser(new HtmlAttributeStringSerializer()).StringToDictionary(attributes);
        }

        [Fact]
        public Task Width_And_Height_Should_Be_Read_Independent_Of_Culture()
        {
            return StaRunner.Run(() =>
            {
                var culture = CultureInfo.CurrentCulture;
                try
                {
                    CultureInfo.CurrentCulture = new CultureInfo("de-DE");

                    var textBlock = new HtmlTextBlock { ImageService = Service, Html = "<img src='https://example.com/a.png' width=\"1.5\" height='2.5'>" };
                    var image = ImageOf(textBlock);

                    Assert.Equal(1.5, image.Width);
                    Assert.Equal(2.5, image.Height);
                }
                finally
                {
                    CultureInfo.CurrentCulture = culture;
                }

                return Task.CompletedTask;
            });
        }

        [Fact]
        public Task Style_Should_Be_Applied_And_Win_Over_Attributes()
        {
            return StaRunner.Run(() =>
            {
                var textBlock = new HtmlTextBlock
                {
                    ImageService = Service,
                    Html = "<img src=\"https://example.com/a.png\" width=\"100\" height=\"100\" style=\"WIDTH: 16px; height:12; margin: 1 2 3 4px; vertical-align: middle; opacity: 0.5; unknown: 1\">",
                };

                var image = ImageOf(textBlock);

                Assert.Multiple(() =>
                {
                    Assert.Equal(16, image.Width);
                    Assert.Equal(12, image.Height);
                    Assert.Equal(new Thickness(4, 1, 2, 3), image.Margin);
                    Assert.Equal(0.5, image.Opacity);
                    Assert.Equal(BaselineAlignment.Center, ContainerOf(textBlock).BaselineAlignment);
                });

                return Task.CompletedTask;
            });
        }

        [Theory]
        [InlineData("margin: 4", 4, 4, 4, 4)]
        [InlineData("margin: 1 2", 2, 1, 2, 1)]
        [InlineData("margin: 1 2 3", 2, 1, 2, 3)]
        public Task Margin_Should_Follow_Css_Shorthand(string style, double left, double top, double right, double bottom)
        {
            return StaRunner.Run(() =>
            {
                var textBlock = new HtmlTextBlock { ImageService = Service, Html = $"<img src='https://example.com/a.png' style='{style}'>" };

                Assert.Equal(new Thickness(left, top, right, bottom), ImageOf(textBlock).Margin);

                return Task.CompletedTask;
            });
        }

        [Theory]
        [InlineData("width: 10em", double.NaN)]
        [InlineData("width: 50%", double.NaN)]
        [InlineData("width: -5", double.NaN)]
        [InlineData("width: abc", double.NaN)]
        [InlineData("opacity: 5", 1d)]
        public Task Invalid_Style_Values_Should_Be_Ignored_Or_Clamped(string style, double expected)
        {
            return StaRunner.Run(() =>
            {
                var textBlock = new HtmlTextBlock { ImageService = Service, Html = $"<img src='https://example.com/a.png' style='{style}'>" };
                var image = ImageOf(textBlock);

                Assert.Equal(expected, style.StartsWith("opacity") ? image.Opacity : image.Width);

                return Task.CompletedTask;
            });
        }

        [Fact]
        public Task Alt_And_Title_Should_Be_Applied()
        {
            return StaRunner.Run(() =>
            {
                var textBlock = new HtmlTextBlock { ImageService = Service, Html = "<img src=\"https://example.com/a.png\" alt=\"A cat\" title=\"Cat picture\">" };
                var image = ImageOf(textBlock);

                Assert.Equal("A cat", AutomationProperties.GetName(image));
                Assert.Equal("Cat picture", image.ToolTip);

                return Task.CompletedTask;
            });
        }

        [Fact]
        public Task Src_Should_Win_Over_Source()
        {
            return StaRunner.Run(async () =>
            {
                var textBlock = new HtmlTextBlock { ImageService = Service, Html = $"<img source=\"https://example.com/old.png\" src=\"{WebImage}\">" };
                var window = Show(textBlock);

                await WaitUntil(() => ImageOf(textBlock).Source is not null);

                await _imageService.Received(1).ProvideImageAsync(WebImage, Arg.Any<ImageSize?>(), Arg.Any<Func<bool, Task>>(), Arg.Any<CancellationToken>());
                await _imageService.DidNotReceive().ProvideImageAsync(new Uri("https://example.com/old.png"), Arg.Any<ImageSize?>(), Arg.Any<Func<bool, Task>>(), Arg.Any<CancellationToken>());

                window.Close();
            });
        }

        [Fact]
        public Task Source_Should_Still_Be_Supported()
        {
            return StaRunner.Run(async () =>
            {
                var textBlock = new HtmlTextBlock { ImageService = Service, Html = $"<img source='{WebImage}'>" };
                var window = Show(textBlock);

                await WaitUntil(() => ImageOf(textBlock).Source is not null);

                window.Close();
            });
        }

        [Fact]
        public Task ImageService_Should_Load_With_Decode_Size()
        {
            return StaRunner.Run(async () =>
            {
                var textBlock = new HtmlTextBlock { ImageService = Service, Html = $"<img src='{WebImage}' width='20' style='height: 10.2px'>" };
                var window = Show(textBlock);

                await WaitUntil(() => ImageOf(textBlock).Source is not null);

                await _imageService.Received(1).ProvideImageAsync(WebImage, new ImageSize(20, 11), Arg.Any<Func<bool, Task>>(), Arg.Any<CancellationToken>());

                window.Close();
            });
        }

        [Fact]
        public Task ImageService_Should_Load_Full_Size_Without_Both_Dimensions()
        {
            return StaRunner.Run(async () =>
            {
                var textBlock = new HtmlTextBlock { ImageService = Service, Html = $"<img src='{WebImage}' width='20'>" };
                var window = Show(textBlock);

                await WaitUntil(() => ImageOf(textBlock).Source is not null);

                await _imageService.Received(1).ProvideImageAsync(WebImage, null, Arg.Any<Func<bool, Task>>(), Arg.Any<CancellationToken>());

                window.Close();
            });
        }

        [Fact]
        public Task ImageService_Should_Take_Priority_Over_ImageLoader()
        {
            return StaRunner.Run(async () =>
            {
                var globalService = Substitute.For<IImageService<BitmapSource>>();
                ImageLoader.AsyncImageLoader = new Lazy<IImageService<BitmapSource>>(() => globalService);

                var textBlock = new HtmlTextBlock { ImageService = Service, Html = $"<img src='{WebImage}'>" };
                var window = Show(textBlock);

                await WaitUntil(() => ImageOf(textBlock).Source is not null);

                await globalService.DidNotReceiveWithAnyArgs().ProvideImageAsync(default, default, default!, default);

                window.Close();
            });
        }

        [Fact]
        public Task ImageLoader_Should_Be_Used_Without_ImageService()
        {
            return StaRunner.Run(async () =>
            {
                ImageLoader.AsyncImageLoader = Service;

                var textBlock = new HtmlTextBlock { Html = $"<img src='{WebImage}'>" };
                var window = Show(textBlock);

                await WaitUntil(() => ImageOf(textBlock).Source is not null);

                await _imageService.Received(1).ProvideImageAsync(WebImage, Arg.Any<ImageSize?>(), Arg.Any<Func<bool, Task>>(), Arg.Any<CancellationToken>());

                window.Close();
            });
        }

        [Fact]
        public Task Changing_ImageService_Should_Rebuild_The_Images()
        {
            return StaRunner.Run(async () =>
            {
                var textBlock = new HtmlTextBlock { Html = $"<img src='{WebImage}'>" };
                var window = Show(textBlock);

                textBlock.ImageService = Service;

                await WaitUntil(() => ImageOf(textBlock).Source is not null);

                await _imageService.Received(1).ProvideImageAsync(WebImage, Arg.Any<ImageSize?>(), Arg.Any<Func<bool, Task>>(), Arg.Any<CancellationToken>());

                window.Close();
            });
        }

        [Fact]
        public Task Without_A_Service_Files_Should_Load_With_BitmapImage()
        {
            var path = CreatePngFile();

            return StaRunner.Run(() =>
            {
                var textBlock = new HtmlTextBlock { Html = $"<img src=\"{path}\">" };

                Assert.IsType<BitmapImage>(ImageOf(textBlock).Source);

                return Task.CompletedTask;
            });
        }

        [Theory]
        [InlineData("pack://application:,,,/missing.png", false)]
        [InlineData("https://example.com/a.png", true)]
        [InlineData("http://example.com/a.png", true)]
        [InlineData("file:///C:/a.png", true)]
        [InlineData("images/a.png", true)]
        [InlineData("ftp://example.com/a.png", false)]
        public void Only_Http_File_And_Relative_Sources_Should_Use_The_ImageService(string uri, bool expected)
        {
            Assert.Equal(expected, HtmlImageFactory.CanUseImageService(new Uri(uri, UriKind.RelativeOrAbsolute)));
        }

        [Fact]
        public Task Pack_Uris_Should_Not_Use_The_ImageService()
        {
            return StaRunner.Run(async () =>
            {
                var textBlock = new HtmlTextBlock { ImageService = Service, Html = "<img src='pack://application:,,,/missing.png' alt='missing'>" };
                var window = Show(textBlock);

                await Dispatcher.Yield(DispatcherPriority.ApplicationIdle);

                await _imageService.DidNotReceiveWithAnyArgs().ProvideImageAsync(default, default, default!, default);

                window.Close();
            });
        }

        [Fact]
        public Task A_Failed_ImageService_Load_Should_Show_The_Alt_Text()
        {
            return StaRunner.Run(async () =>
            {
                _imageService.ProvideImageAsync(Arg.Any<Uri?>(), Arg.Any<ImageSize?>(), Arg.Any<Func<bool, Task>>(), Arg.Any<CancellationToken>())
                    .Returns(Task.FromResult<BitmapSource?>(null));

                var textBlock = new HtmlTextBlock { ImageService = Service, Html = $"before <img src='{WebImage}' alt='A cat'> after" };
                var window = Show(textBlock);

                await WaitUntil(() => ContainerOf(textBlock).Child is TextBlock);

                Assert.Equal("A cat", Assert.IsType<TextBlock>(ContainerOf(textBlock).Child).Text);

                window.Close();
            });
        }

        [Fact]
        public Task A_Failed_ImageService_Load_Without_Alt_Should_Show_Nothing()
        {
            return StaRunner.Run(async () =>
            {
                _imageService.ProvideImageAsync(Arg.Any<Uri?>(), Arg.Any<ImageSize?>(), Arg.Any<Func<bool, Task>>(), Arg.Any<CancellationToken>())
                    .Returns(Task.FromResult<BitmapSource?>(null));

                var textBlock = new HtmlTextBlock { ImageService = Service, Html = $"<img src='{WebImage}'>" };
                var window = Show(textBlock);

                await WaitUntil(() => ContainerOf(textBlock).Child is null);

                window.Close();
            });
        }

        [Fact]
        public Task A_Missing_File_Should_Show_The_Alt_Text()
        {
            var path = Path.Combine(_directory, "missing.png");

            return StaRunner.Run(() =>
            {
                var textBlock = new HtmlTextBlock { Html = $"before <img src=\"{path}\" alt=\"A cat\"> after" };

                Assert.Equal("before A cat after", HtmlTextBlockTests.TextOf(textBlock));

                return Task.CompletedTask;
            });
        }

        [Fact]
        public Task An_Image_Without_Source_Should_Show_The_Alt_Text()
        {
            return StaRunner.Run(() =>
            {
                var textBlock = new HtmlTextBlock { Html = "<img alt='A cat'>" };

                Assert.Equal("A cat", HtmlTextBlockTests.TextOf(textBlock));

                return Task.CompletedTask;
            });
        }

        [Fact]
        public Task An_Image_With_Href_Should_Be_Wrapped_In_A_Hyperlink()
        {
            return StaRunner.Run(() =>
            {
                var textBlock = new HtmlTextBlock { ImageService = Service, Html = $"<img src='{WebImage}' href=\"https://example.com/\">" };

                var link = Assert.IsType<Hyperlink>(Assert.Single(textBlock.Inlines));
                Assert.Equal(new Uri("https://example.com/"), link.NavigateUri);

                return Task.CompletedTask;
            });
        }

        [Fact]
        public Task Double_Quoted_Href_Should_Not_Keep_Its_Quotes()
        {
            return StaRunner.Run(() =>
            {
                var textBlock = new HtmlTextBlock { Html = "<a href=\"https://example.com/a b\">link</a>" };

                var link = Descendants(textBlock.Inlines).OfType<Hyperlink>().Single();
                Assert.Equal("https://example.com/a b", link.ToolTip);

                return Task.CompletedTask;
            });
        }

        [Fact]
        public Task Relative_Image_Sources_Should_Be_Resolved_Against_The_BaseUri()
        {
            var path = CreatePngFile();

            return StaRunner.Run(() =>
            {
                // the uri of the html file itself, its folder holds the image
                var textBlock = new HtmlTextBlock
                {
                    BaseUri = new Uri(Path.Combine(_directory, "page.html")),
                    Html = "<img src='image.png' alt='missing'>",
                };

                var bitmap = Assert.IsType<BitmapImage>(ImageOf(textBlock).Source);
                Assert.Equal(new Uri(path), bitmap.UriSource);

                return Task.CompletedTask;
            });
        }

        [Fact]
        public Task Relative_Image_Sources_Should_Reach_The_ImageService_Resolved()
        {
            return StaRunner.Run(async () =>
            {
                var textBlock = new HtmlTextBlock
                {
                    ImageService = Service,
                    BaseUri = new Uri("https://example.com/docs/"),
                    Html = "<img src='images/a.png'>",
                };
                var window = Show(textBlock);

                await WaitUntil(() => ImageOf(textBlock).Source is not null);

                await _imageService.Received(1).ProvideImageAsync(new Uri("https://example.com/docs/images/a.png"), Arg.Any<ImageSize?>(), Arg.Any<Func<bool, Task>>(), Arg.Any<CancellationToken>());

                window.Close();
            });
        }

        [Theory]
        [InlineData("https://example.com/docs/", "page.html", "https://example.com/docs/page.html", "https://example.com/docs/page.html")]
        [InlineData("https://example.com/docs/index.html", "../about.html", "https://example.com/about.html", "https://example.com/about.html")]
        [InlineData("https://example.com/docs/", "https://other.com/a b", "https://other.com/a b", "https://other.com/a%20b")]
        [InlineData(null, "page.html", "page.html", "page.html")]
        public Task Relative_Links_Should_Be_Resolved_Against_The_BaseUri(string? baseUri, string href, string toolTip, string navigateUri)
        {
            return StaRunner.Run(() =>
            {
                var textBlock = new HtmlTextBlock
                {
                    BaseUri = baseUri is null ? null : new Uri(baseUri),
                    Html = $"<a href=\"{href}\">link</a>",
                };

                var link = Descendants(textBlock.Inlines).OfType<Hyperlink>().Single();

                Assert.Multiple(() =>
                {
                    Assert.Equal(navigateUri, link.NavigateUri!.IsAbsoluteUri ? link.NavigateUri.AbsoluteUri : link.NavigateUri.OriginalString);
                    Assert.Equal(toolTip, link.ToolTip);
                });

                return Task.CompletedTask;
            });
        }

        [Fact]
        public Task A_Relative_BaseUri_Should_Be_Ignored()
        {
            return StaRunner.Run(() =>
            {
                var textBlock = new HtmlTextBlock
                {
                    BaseUri = new Uri("docs/", UriKind.Relative),
                    Html = "<a href='page.html'>link</a>",
                };

                Assert.Equal(new Uri("page.html", UriKind.Relative), Descendants(textBlock.Inlines).OfType<Hyperlink>().Single().NavigateUri);

                return Task.CompletedTask;
            });
        }

        [Fact]
        public Task Changing_The_BaseUri_Should_Rebuild_The_Content()
        {
            return StaRunner.Run(() =>
            {
                var textBlock = new HtmlTextBlock { Html = "<a href='page.html'>link</a>" };

                textBlock.BaseUri = new Uri("https://example.com/");

                Assert.Equal(new Uri("https://example.com/page.html"), Descendants(textBlock.Inlines).OfType<Hyperlink>().Single().NavigateUri);

                return Task.CompletedTask;
            });
        }

        [Theory]
        [InlineData("<font color=\"red\">x</font>")]
        [InlineData("<font color='#FF0000'>x</font>")]
        [InlineData("<span style=\"color: #f00\">x</span>")]
        [InlineData("<color color=\"Red\">x</color>")]
        public Task Text_Color_Should_Be_Applied(string html)
        {
            return StaRunner.Run(() =>
            {
                var textBlock = new HtmlTextBlock { Html = html };

                Assert.Equal(Colors.Red, ColorOf(textBlock, "x"));

                return Task.CompletedTask;
            });
        }

        [Fact]
        public Task Innermost_Color_Should_Win_And_Outer_Color_Should_Return()
        {
            return StaRunner.Run(() =>
            {
                var textBlock = new HtmlTextBlock { Html = "<font color='red'>a<span style='color: blue'>b</span>c</font>" };

                Assert.Multiple(() =>
                {
                    Assert.Equal(Colors.Red, ColorOf(textBlock, "a"));
                    Assert.Equal(Colors.Blue, ColorOf(textBlock, "b"));
                    Assert.Equal(Colors.Red, ColorOf(textBlock, "c"));
                });

                return Task.CompletedTask;
            });
        }

        [Fact]
        public Task Invalid_Colors_Should_Be_Ignored()
        {
            return StaRunner.Run(() =>
            {
                var textBlock = new HtmlTextBlock { Html = "<font color='red'>a<span style='color: notacolor'>b</span></font><font color='#12'>c</font>" };

                Assert.Multiple(() =>
                {
                    Assert.Equal(Colors.Red, ColorOf(textBlock, "b"));
                    Assert.Null(Descendants(textBlock.Inlines).OfType<Run>().Single(p => p.Text == "c").ReadLocalValue(TextElement.ForegroundProperty) as SolidColorBrush);
                });

                return Task.CompletedTask;
            });
        }

        [Theory]
        [InlineData("a <span>b</span> c", "a b c")]
        [InlineData("a <span class='x'><b>b</b></span> c", "a b c")]
        public Task Spans_Should_Render_As_Before(string html, string expected)
        {
            return StaRunner.Run(() =>
            {
                var textBlock = new HtmlTextBlock { Html = html };

                Assert.Equal(expected, HtmlTextBlockTests.TextOf(textBlock));

                return Task.CompletedTask;
            });
        }
    }
}
