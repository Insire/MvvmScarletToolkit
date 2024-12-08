using Jot;
using Jot.Storage;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IO;
using MvvmScarletToolkit.ImageLoading;
using MvvmScarletToolkit.Observables;
using MvvmScarletToolkit.Wpf.Samples.Features;
using MvvmScarletToolkit.Wpf.Samples.Features.Image;
using MvvmScarletToolkit.Wpf.Samples.Features.Process;
using System;
using System.Linq;
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

        protected override async void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            _tracker.Configure<MainWindow>()
                .Id(w => w.Name, $"[Width={SystemParameters.VirtualScreenWidth},Height{SystemParameters.VirtualScreenHeight}]")
                .Properties(w => new { w.Height, w.Width, w.Left, w.Top, w.WindowState })
            .PersistOn(nameof(Window.Closing))
                .StopTrackingOn(nameof(Window.Closing));

            var environmentInformationProvider = new EnvironmentInformationProvider();
            ConfigureImageLoading(environmentInformationProvider);

            var httpClient = new HttpClient();

            var navigation = new NavigationViewModel(
                SynchronizationContext.Current!,
                ScarletCommandBuilder.Default,
                new LocalizationsViewModel(new ScarletLocalizationProvider()),
                environmentInformationProvider,
                httpClient);

            var processingImagesViewModel = navigation.Items
                .Where(p => p.Content?.GetType() == typeof(ProcessingImagesViewModel))
                .Select(static p => (ProcessingImagesViewModel)p.Content!)
                .Single();

            await processingImagesViewModel.Initialize(CancellationToken.None);

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
            var factory = new Lazy<IImageService<BitmapSource>>(() =>
            {
                var loggerFactory = LoggerFactory.Create(builder => builder.AddDebug().SetMinimumLevel(LogLevel.Trace));
                var factory = new ImageFactory(loggerFactory.CreateLogger<ImageFactory>(), new SemaphoreSlim(Environment.ProcessorCount));

                return new ImageService<BitmapSource>(
                            loggerFactory.CreateLogger<ImageService<BitmapSource>>(),
                            factory,
                            new ImageDataProvider(loggerFactory.CreateLogger<ImageDataProvider>(), _recyclableMemoryStreamManager, _httpClient),
                            new ImageDataFileystemCache(loggerFactory.CreateLogger<ImageDataFileystemCache>(), _recyclableMemoryStreamManager, new ImageDataFileystemCacheOptions() { CacheDirectoryPath = environmentInformationProvider.GetRawImagesFolderPath(), CreateFolder = true, IsEnabled = true }),
                            new ImageFilesystemCache<BitmapSource>(loggerFactory.CreateLogger<ImageFilesystemCache<BitmapSource>>(), factory, _recyclableMemoryStreamManager, new ImageFilesystemCacheOptions() { IsEnabled = true, CacheDirectoryPath = environmentInformationProvider.GetEncodedImagesFolderPath(), CreateFolder = true }),
                            new ImageDataMemoryCache(loggerFactory.CreateLogger<ImageDataMemoryCache>(), _memoryCache, _recyclableMemoryStreamManager, new ImageDataMemoryCacheOptions() { IsEnabled = true }),
                            new ImageMemoryCache<BitmapSource>(loggerFactory.CreateLogger<ImageMemoryCache<BitmapSource>>(), _memoryCache, _recyclableMemoryStreamManager, new ImageMemoryCacheOptions() { IsEnabled = true }),
                            _memoryCache,
                            _recyclableMemoryStreamManager,
                            new ImageServiceOptions() { DefaultHeight = 300, DefaultWidth = 300 });
            });

            ImageLoader.AsyncImageLoader = factory;
            AsyncImageLoadingBehavior.Loader = factory;
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
