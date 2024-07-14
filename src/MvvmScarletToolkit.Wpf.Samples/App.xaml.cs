using Jot;
using Jot.Storage;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IO;
using MvvmScarletToolkit.Abstractions.ImageLoading;
using MvvmScarletToolkit.ImageLoading;
using MvvmScarletToolkit.Observables;
using System;
using System.Net.Http;
using System.Runtime.Versioning;
using System.Threading;
using System.Windows;
using System.Windows.Media.Imaging;

namespace MvvmScarletToolkit.Wpf.Samples
{
    [SupportedOSPlatform("windows7.0")]
    public partial class App : Application
    {
        private readonly Tracker _tracker;

        public App()
        {
            _tracker = new Tracker(new JsonFileStore(Environment.SpecialFolder.ApplicationData));
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            _tracker.Configure<MainWindow>()
                .Id(w => w.Name, $"[Width={SystemParameters.VirtualScreenWidth},Height{SystemParameters.VirtualScreenHeight}]")
                .Properties(w => new { w.Height, w.Width, w.Left, w.Top, w.WindowState })
            .PersistOn(nameof(Window.Closing))
                .StopTrackingOn(nameof(Window.Closing));

            var environmentInformationProvider = new EnvironmentInformationProvider();
            ConfigureImageLoading(environmentInformationProvider);

            var navigation = new NavigationViewModel(
                SynchronizationContext.Current!,
                ScarletCommandBuilder.Default,
                new LocalizationsViewModel(new ScarletLocalizationProvider()),
                environmentInformationProvider);

            var window = new MainWindow(_tracker, navigation);
            window.Show();
        }

        protected override async void OnExit(ExitEventArgs e)
        {
            base.OnExit(e);

            await ScarletExitService.Default.ShutDown().ConfigureAwait(false);

            _tracker.PersistAll();
        }

        private static void ConfigureImageLoading(EnvironmentInformationProvider environmentInformationProvider)
        {
            var loggerFactory = LoggerFactory.Create(builder => builder.AddDebug());
            var logger = loggerFactory.CreateLogger<DiskCachedWebImageLoader<BitmapSource>>();

            var options = new DiskCachedWebImageLoaderOptions()
            {
                CacheFolder = environmentInformationProvider.GetImagesFolderPath(),
                CreateFolder = true,
                DisposeHttpClient = true,
            };

            var httpClient = new HttpClient();
            var manager = new RecyclableMemoryStreamManager();
            var imageFactory = new ImageFactory();
            var memoryCache = new MemoryCache(new MemoryCacheOptionsWrapper(new MemoryCacheOptions()));
            var imageLoader = new DiskCachedWebImageLoader<BitmapSource>(
                logger,
                memoryCache,
                httpClient,
                manager,
                imageFactory,
                options);

            ImageLoader.Manager = manager;
            ImageLoader.ImageFactory = imageFactory;
            ImageLoader.AsyncImageLoader = new Lazy<IAsyncImageLoader<BitmapSource>>(imageLoader);
        }

        private sealed class MemoryCacheOptionsWrapper : IOptions<MemoryCacheOptions>
        {
            public MemoryCacheOptions Value { get; }

            public MemoryCacheOptionsWrapper(MemoryCacheOptions value)
            {
                Value = value;
            }
        }
    }
}
