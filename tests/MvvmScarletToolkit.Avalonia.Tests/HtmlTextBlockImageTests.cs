using Avalonia;
using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Controls.Documents;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using MvvmScarletToolkit.Avalonia.Tests.Util;
using System.Globalization;

namespace MvvmScarletToolkit.Avalonia.Tests
{
    // AsyncImageLoadingBehavior.Loader is static, tests that depend on it must not run in parallel with other users of it
    [Collection(AsyncImageLoadingBehaviorTests.LoaderCollection)]
    public sealed class HtmlTextBlockImageTests : TraceTestBase, IDisposable
    {
        private static readonly Uri WebImage = new("https://example.com/image.png");

        private readonly AvaloniaPlatformFixture _platform;
        private readonly IImageService<Bitmap> _imageService = Substitute.For<IImageService<Bitmap>>();
        private readonly string _directory;

        // TestContext.Current does not flow to the Avalonia UI thread, capture what is needed up front
        private readonly CancellationToken _cancellationToken = TestContext.Current.CancellationToken;

        public HtmlTextBlockImageTests(AvaloniaPlatformFixture platform)
        {
            _platform = platform;
            AsyncImageLoadingBehavior.Loader = null;

            _imageService.ProvideImageAsync(Arg.Any<Uri>(), Arg.Any<ImageSize?>(), Arg.Any<Func<bool, Task>>(), Arg.Any<CancellationToken>())
                .Returns(_ => Task.FromResult<Bitmap?>(TestBitmaps.Create(10, 10)));

            _directory = Path.Combine(Path.GetTempPath(), nameof(HtmlTextBlockImageTests), Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(_directory);
        }

        public void Dispose()
        {
            AsyncImageLoadingBehavior.Loader = null;
            Directory.Delete(_directory, true);
        }

        private Lazy<IImageService<Bitmap>> Service => new(() => _imageService);

        private string CreatePngFile()
        {
            var path = Path.Combine(_directory, "image.png");
            using var png = TestBitmaps.CreatePng(4, 4);
            File.WriteAllBytes(path, png.ToArray());

            return path;
        }

        private static InlineUIContainer ContainerOf(TextBlock textBlock) => HtmlTextBlockTests.Descendants(textBlock.Inlines!).OfType<InlineUIContainer>().Single();

        private static Decorator HostOf(TextBlock textBlock) => Assert.IsType<Decorator>(ContainerOf(textBlock).Child);

        private static Image ImageOf(TextBlock textBlock) => Assert.IsType<Image>(HostOf(textBlock).Child);

        private static Window Show(Control content)
        {
            var window = new Window { Content = content };
            window.Show();

            return window;
        }

        private async Task WaitUntil(Func<bool> condition)
        {
            using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(5));
            while (!condition())
            {
                timeout.Token.ThrowIfCancellationRequested();
                Dispatcher.UIThread.RunJobs();
                await Task.Delay(5, _cancellationToken);
            }
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
        public Task Width_And_Height_Should_Be_Read_Independent_Of_Culture()
        {
            return Run(() =>
            {
                var culture = CultureInfo.CurrentCulture;
                try
                {
                    CultureInfo.CurrentCulture = new CultureInfo("de-DE");

                    var textBlock = new HtmlTextBlock { ImageService = Service, Html = "<img src='https://example.com/a.png' width='10.5' height='20px'>" };
                    var image = ImageOf(textBlock);

                    Assert.Multiple(() =>
                    {
                        Assert.Equal(10.5, image.Width);
                        Assert.Equal(20, image.Height);
                    });
                }
                finally
                {
                    CultureInfo.CurrentCulture = culture;
                }
            });
        }

        [Fact]
        public Task Style_Should_Be_Applied_And_Win_Over_Attributes()
        {
            return Run(() =>
            {
                var textBlock = new HtmlTextBlock { ImageService = Service, Html = "<img src='https://example.com/a.png' width='5' style='width: 16px; height: 8; margin: 1 2 3 4; opacity: 0.5; vertical-align: middle'>" };
                var image = ImageOf(textBlock);

                Assert.Multiple(() =>
                {
                    Assert.Equal(16, image.Width);
                    Assert.Equal(8, image.Height);
                    Assert.Equal(new Thickness(4, 1, 2, 3), image.Margin);
                    Assert.Equal(0.5, image.Opacity);
                    Assert.Equal(BaselineAlignment.Center, ContainerOf(textBlock).BaselineAlignment);
                });
            });
        }

        [Theory]
        [InlineData("width: 10em")]
        [InlineData("width: 50%")]
        [InlineData("width: -5")]
        [InlineData("width: abc")]
        public Task Invalid_Lengths_Should_Be_Ignored(string style)
        {
            return Run(() =>
            {
                var textBlock = new HtmlTextBlock { ImageService = Service, Html = $"<img src='https://example.com/a.png' style='{style}'>" };

                Assert.True(double.IsNaN(ImageOf(textBlock).Width));
            });
        }

        [Fact]
        public Task Alt_And_Title_Should_Be_Applied()
        {
            return Run(() =>
            {
                var textBlock = new HtmlTextBlock { ImageService = Service, Html = "<img src='https://example.com/a.png' alt='Alt' title='Title'>" };
                var image = ImageOf(textBlock);

                Assert.Multiple(() =>
                {
                    Assert.Equal("Alt", AutomationProperties.GetName(image));
                    Assert.Equal("Title", ToolTip.GetTip(image));
                });
            });
        }

        [Fact]
        public Task ImageService_Should_Load_With_Decode_Size()
        {
            return _platform.RunOnUIThread(async () =>
            {
                var textBlock = new HtmlTextBlock { ImageService = Service, Html = $"<img src='{WebImage}' width='16' height='8.5'>" };
                var window = Show(textBlock);

                await WaitUntil(() => ImageOf(textBlock).Source is not null);

                await _imageService.Received(1).ProvideImageAsync(WebImage, new ImageSize(16, 9), Arg.Any<Func<bool, Task>>(), Arg.Any<CancellationToken>());
                window.Close();
            });
        }

        [Fact]
        public Task Global_Loader_Should_Be_Used_Without_ImageService()
        {
            return _platform.RunOnUIThread(async () =>
            {
                AsyncImageLoadingBehavior.Loader = Service;

                var textBlock = new HtmlTextBlock { Html = $"<img src='{WebImage}'>" };
                var window = Show(textBlock);

                await WaitUntil(() => ImageOf(textBlock).Source is not null);

                await _imageService.Received(1).ProvideImageAsync(WebImage, null, Arg.Any<Func<bool, Task>>(), Arg.Any<CancellationToken>());
                window.Close();
            });
        }

        [Fact]
        public Task ImageService_Should_Take_Priority_Over_The_Global_Loader()
        {
            return _platform.RunOnUIThread(async () =>
            {
                var global = Substitute.For<IImageService<Bitmap>>();
                AsyncImageLoadingBehavior.Loader = new Lazy<IImageService<Bitmap>>(() => global);

                var textBlock = new HtmlTextBlock { ImageService = Service, Html = $"<img src='{WebImage}'>" };
                var window = Show(textBlock);

                await WaitUntil(() => ImageOf(textBlock).Source is not null);

                await global.DidNotReceiveWithAnyArgs().ProvideImageAsync(default!, default, default!, default);
                window.Close();
            });
        }

        [Fact]
        public Task Relative_Image_Sources_Should_Reach_The_ImageService_Resolved()
        {
            return _platform.RunOnUIThread(async () =>
            {
                var textBlock = new HtmlTextBlock
                {
                    ImageService = Service,
                    BaseUri = new Uri("https://example.com/docs/index.html"),
                    Html = "<img src='images/a.png'>",
                };
                var window = Show(textBlock);

                await WaitUntil(() => ImageOf(textBlock).Source is not null);

                await _imageService.Received(1).ProvideImageAsync(new Uri("https://example.com/docs/images/a.png"), null, Arg.Any<Func<bool, Task>>(), Arg.Any<CancellationToken>());
                window.Close();
            });
        }

        [Theory]
        [InlineData("Alt", "Alt")]
        [InlineData(null, null)]
        public Task A_Failed_ImageService_Load_Should_Show_The_Alt_Text(string? alt, string? expected)
        {
            return _platform.RunOnUIThread(async () =>
            {
                _imageService.ProvideImageAsync(WebImage, Arg.Any<ImageSize?>(), Arg.Any<Func<bool, Task>>(), Arg.Any<CancellationToken>())
                    .Returns(Task.FromResult<Bitmap?>(null));

                var altAttribute = alt is null ? string.Empty : $" alt='{alt}'";
                var textBlock = new HtmlTextBlock { ImageService = Service, Html = $"<img src='{WebImage}'{altAttribute}>" };
                var window = Show(textBlock);

                await WaitUntil(() => HostOf(textBlock).Child is not Image);

                Assert.Equal(expected, (HostOf(textBlock).Child as TextBlock)?.Text);
                window.Close();
            });
        }

        [Fact]
        public Task Without_A_Service_Files_Should_Load_Directly()
        {
            return Run(() =>
            {
                var path = CreatePngFile();

                var textBlock = new HtmlTextBlock { Html = $"<img src='{new Uri(path)}'>" };

                Assert.Equal(new PixelSize(4, 4), Assert.IsType<Bitmap>(ImageOf(textBlock).Source).PixelSize);
            });
        }

        [Fact]
        public Task Relative_Files_Should_Be_Resolved_Against_The_BaseUri()
        {
            return Run(() =>
            {
                CreatePngFile();

                var textBlock = new HtmlTextBlock
                {
                    BaseUri = new Uri(Path.Combine(_directory, "page.html")),
                    Html = "<img src='image.png'>",
                };

                Assert.IsType<Bitmap>(ImageOf(textBlock).Source);
            });
        }

        [Theory]
        [InlineData("<img src='missing.png' alt='Alt'>")]
        [InlineData("<img alt='Alt'>")]
        [InlineData("<img src='https://example.com/a.png' alt='Alt'>")] // web images need a service
        public Task Images_That_Cannot_Load_Should_Show_The_Alt_Text(string html)
        {
            return Run(() =>
            {
                var textBlock = new HtmlTextBlock { Html = html };

                Assert.Equal("Alt", HtmlTextBlockTests.TextOf(textBlock));
            });
        }

        [Theory]
        [InlineData("avares://missing/a.png", false)]
        [InlineData("https://example.com/a.png", true)]
        [InlineData("http://example.com/a.png", true)]
        [InlineData("file:///C:/a.png", true)]
        [InlineData("images/a.png", true)]
        [InlineData("ftp://example.com/a.png", false)]
        public void Only_Web_File_And_Relative_Sources_Should_Use_The_ImageService(string source, bool expected)
        {
            Assert.Equal(expected, HtmlImageFactory.CanUseImageService(new Uri(source, UriKind.RelativeOrAbsolute)));
        }

        [Fact]
        public Task An_Image_With_Href_Should_Be_Wrapped_In_A_Link()
        {
            return Run(() =>
            {
                var path = CreatePngFile();

                var textBlock = new HtmlTextBlock { OpenLinksInBrowser = true, Html = $"<img src='{new Uri(path)}' href=\"https://example.com/\" style='vertical-align: top'>" };

                var container = Assert.IsType<InlineUIContainer>(Assert.Single(textBlock.Inlines!));
                var link = Assert.IsType<HyperlinkButton>(container.Child);

                Assert.Multiple(() =>
                {
                    Assert.IsType<Image>(Assert.IsType<Decorator>(link.Content).Child);
                    Assert.Equal(new Uri("https://example.com/"), link.CommandParameter);
                    Assert.Same(textBlock.LinkCommand, link.Command);
                    Assert.Equal(BaselineAlignment.Top, container.BaselineAlignment);
                });
            });
        }

        [Fact]
        public Task Changing_ImageService_Should_Rebuild_The_Images()
        {
            return Run(() =>
            {
                var textBlock = new HtmlTextBlock { Html = $"<img src='{WebImage}' alt='Alt'>" };
                Assert.Equal("Alt", HtmlTextBlockTests.TextOf(textBlock));

                textBlock.ImageService = Service;

                Assert.IsType<Image>(HostOf(textBlock).Child);
            });
        }
    }
}
