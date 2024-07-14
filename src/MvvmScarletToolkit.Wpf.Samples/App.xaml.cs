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
        private readonly HttpClient _httpClient;
        private readonly RecyclableMemoryStreamManager _recyclableMemoryStreamManager;
        private readonly MemoryCache _memoryCache;

        public App()
        {
            _tracker = new Tracker(new JsonFileStore(Environment.SpecialFolder.ApplicationData));
            _httpClient = new HttpClient();
            _recyclableMemoryStreamManager = new RecyclableMemoryStreamManager();
            _memoryCache = new MemoryCache(new MemoryCacheOptionsWrapper(new MemoryCacheOptions()));
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

        private void ConfigureImageLoading(EnvironmentInformationProvider environmentInformationProvider)
        {
            ImageLoader.AsyncImageLoader = new Lazy<IAsyncImageLoader<BitmapSource>>(() =>
            {
                var loggerFactory = LoggerFactory.Create(builder => builder.AddDebug().SetMinimumLevel(LogLevel.Trace));

                return new ImageLoader<BitmapSource>(
                            loggerFactory.CreateLogger<ImageLoader<BitmapSource>>(),
                            new ImageFactory(),
                            new ImageDataProvider(loggerFactory.CreateLogger<ImageDataProvider>(), _recyclableMemoryStreamManager, _httpClient),
                            new DiskCacheImageDataProvider(loggerFactory.CreateLogger<DiskCacheImageDataProvider>(), _recyclableMemoryStreamManager, new DiskCacheImageDataProviderOptions() { CacheDirectoryPath = environmentInformationProvider.GetImagesFolderPath(), CreateFolder = true, IsEnabled = true }),
                            new MemoryCacheImageDataProvider(loggerFactory.CreateLogger<MemoryCacheImageDataProvider>(), _memoryCache, _recyclableMemoryStreamManager, new MemoryCacheImageDataProviderOptions() { IsEnabled = true }),
                            new MemoryCacheImageProvider<BitmapSource>(loggerFactory.CreateLogger<MemoryCacheImageProvider<BitmapSource>>(), _memoryCache, _recyclableMemoryStreamManager, new MemoryCacheImageProviderOptions() { IsEnabled = true }),
                            _memoryCache,
                            new ImageLoaderOptions() { DefaultHeight = 300, DefaultWidth = 300 });
            });
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
