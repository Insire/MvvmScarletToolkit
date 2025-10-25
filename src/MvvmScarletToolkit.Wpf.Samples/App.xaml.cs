using CommunityToolkit.Mvvm.DependencyInjection;
using Jot;
using Jot.Storage;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.IO;
using MvvmScarletToolkit.Core.Samples.Features;
using MvvmScarletToolkit.Core.Samples.Features.AsyncState;
using MvvmScarletToolkit.Core.Samples.Features.Busy;
using MvvmScarletToolkit.Core.Samples.Features.Process;
using MvvmScarletToolkit.Core.Samples.Features.Virtualization;
using MvvmScarletToolkit.ImageLoading;
using MvvmScarletToolkit.ImageLoading.Caches;
using MvvmScarletToolkit.ImageLoading.Caches.Data;
using MvvmScarletToolkit.Observables;
using MvvmScarletToolkit.Wpf.Features.FileSystemBrowser;
using MvvmScarletToolkit.Wpf.Features.FileSystemBrowser.Interfaces;
using MvvmScarletToolkit.Wpf.Samples.Features;
using MvvmScarletToolkit.Wpf.Samples.Features.DataGrid;
using MvvmScarletToolkit.Wpf.Samples.Features.Geometry;
using MvvmScarletToolkit.Wpf.Samples.Features.Image;
using Serilog;
using System;
using System.IO;
using System.Net.Http;
using System.Reactive.Concurrency;
using System.Runtime.Versioning;
using System.Threading;
using System.Windows;
using System.Windows.Media.Imaging;

namespace MvvmScarletToolkit.Wpf.Samples
{
    [SupportedOSPlatform("windows7.0")]
    public partial class App : Application
    {
        public App()
        {
            Ioc.Default.ConfigureServices(ConfigureServices());

            AsyncImageLoadingBehavior.Loader = Ioc.Default.GetRequiredService<Lazy<IImageService<BitmapSource>>>();
        }

        private static ServiceProvider ConfigureServices()
        {
            var serviceCollection = new ServiceCollection();

            var environmentInformationProvider = new EnvironmentInformationProvider();
            var tracker = new Tracker(new JsonFileStore(Environment.SpecialFolder.ApplicationData));

            tracker.Configure<MainWindow>()
                .Id(w => w.Name, $"[Width={SystemParameters.VirtualScreenWidth},Height{SystemParameters.VirtualScreenHeight}]")
                .Properties(w => new { w.Height, w.Width, w.Left, w.Top, w.WindowState })
                .PersistOn(nameof(Window.Closing))
                .StopTrackingOn(nameof(Window.Closing));

            serviceCollection.AddSingleton<MainWindow>();
            serviceCollection.AddSingleton<NavigationViewModel>();
            serviceCollection.AddSingleton<DataGridViewModel>();
            serviceCollection.AddSingleton<ImageViewModelProvider>();
            serviceCollection.AddSingleton<LocalizationsViewModel>();
            serviceCollection.AddSingleton<ProcessingImagesViewModel>();
            serviceCollection.AddSingleton<DataEntriesViewModel>();
            serviceCollection.AddSingleton<AsyncStateListViewModel>();
            serviceCollection.AddSingleton<ProgressViewModel>();
            serviceCollection.AddSingleton<FileSystemViewModel>();
            serviceCollection.AddSingleton<FilePickerViewModel>();
            serviceCollection.AddSingleton<FolderPickerViewModel>();
            serviceCollection.AddSingleton<IFileSystemViewModelFactory, FileSystemViewModelFactory>();
            serviceCollection.AddSingleton(FileSystemOptionsViewModel.Default);
            serviceCollection.AddSingleton<BusyViewModel>();
            serviceCollection.AddSingleton<GeometryRenderViewModel>();
            serviceCollection.AddSingleton<PasswordViewModel>();
            serviceCollection.AddSingleton<DialogViewModel>();
            serviceCollection.AddSingleton<ProcessViewModel>();
            serviceCollection.AddSingleton<ToastsViewModel>();

            serviceCollection.AddSingleton<IScheduler>(provider => new SynchronizationContextScheduler(provider.GetRequiredService<SynchronizationContext>()));
            serviceCollection.AddSingleton<ILocalizationProvider, ScarletLocalizationProvider>();
            serviceCollection.AddSingleton(_ => SynchronizationContext.Current!);
            serviceCollection.AddSingleton(ScarletCommandBuilder.Default);
            serviceCollection.AddSingleton(tracker);
            serviceCollection.AddSingleton<HttpClient>();
            serviceCollection.AddSingleton<RecyclableMemoryStreamManager>();
            serviceCollection.AddSingleton(new MemoryCache(new MemoryCacheOptionsWrapper(new MemoryCacheOptions())));
            serviceCollection.AddSingleton(environmentInformationProvider);
            serviceCollection.AddLogging(builder =>
                builder.AddSerilog(new LoggerConfiguration()
                    .MinimumLevel.Verbose()
                    .Enrich.FromLogContext()
                    .WriteTo.Debug(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] <{SourceContext}> {Message:lj}{NewLine}{Exception}")
                    .WriteTo.File(Path.Combine(environmentInformationProvider.GetLogsFolderPath(), "logs.txt"), outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] <{SourceContext}> {Message:lj}{NewLine}{Exception}")
                    .CreateLogger(), dispose: true));

            serviceCollection.AddSingleton<ImageService<BitmapSource>>();
            serviceCollection.AddSingleton<IImageDataProvider, ImageDataProvider>();
            serviceCollection.AddSingleton<IImageFactory<BitmapSource>, WpfImageFactory>();
            serviceCollection.AddSingleton<IImageDataFileystemCache, ImageDataFileystemCache>();
            serviceCollection.AddSingleton<IImageFilesystemCache<BitmapSource>, ImageFilesystemCache<BitmapSource>>();
            serviceCollection.AddSingleton<IImageDataMemoryCache, ImageDataMemoryCache>();
            serviceCollection.AddSingleton<IImageMemoryCache<BitmapSource>, ImageMemoryCache<BitmapSource>>();
            serviceCollection.AddSingleton(new SemaphoreSlim(Environment.ProcessorCount));
            serviceCollection.AddSingleton(new ImageDataFileystemCacheOptions() { CacheDirectoryPath = environmentInformationProvider.GetRawImagesFolderPath(), CreateFolder = true, IsEnabled = true });
            serviceCollection.AddSingleton(new ImageFilesystemCacheOptions() { IsEnabled = true, CacheDirectoryPath = environmentInformationProvider.GetEncodedImagesFolderPath(), CreateFolder = true });
            serviceCollection.AddSingleton(new ImageDataMemoryCacheOptions() { IsEnabled = true });
            serviceCollection.AddSingleton(new ImageMemoryCacheOptions() { IsEnabled = true });
            serviceCollection.AddSingleton(new ImageServiceOptions() { DefaultHeight = 300, DefaultWidth = 300 });
            serviceCollection.AddSingleton(provider => new Lazy<IImageService<BitmapSource>>(() => provider.GetRequiredService<ImageService<BitmapSource>>()));
            serviceCollection.AddMemoryCache();

            return serviceCollection.BuildServiceProvider();
        }

        protected override async void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            var processingImagesViewModel = Ioc.Default.GetRequiredService<ProcessingImagesViewModel>();
            await processingImagesViewModel.Initialize(CancellationToken.None);

            var window = Ioc.Default.GetRequiredService<MainWindow>();
            window.Show();
        }

        protected override async void OnExit(ExitEventArgs e)
        {
            base.OnExit(e);

            await ScarletExitService.Default.ShutDown().ConfigureAwait(false);
            Ioc.Default.GetRequiredService<Tracker>().PersistAll();
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
