using CommunityToolkit.Mvvm.DependencyInjection;
using Jot;
using Jot.Storage;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IO;
using MvvmScarletToolkit.Core.Samples.Features;
using MvvmScarletToolkit.ImageLoading;
using MvvmScarletToolkit.ImageLoading.Caches;
using MvvmScarletToolkit.ImageLoading.Caches.Data;
using MvvmScarletToolkit.Observables;
using MvvmScarletToolkit.Wpf.Features.FileSystemBrowser;
using MvvmScarletToolkit.Wpf.Features.FileSystemBrowser.Interfaces;
using MvvmScarletToolkit.Wpf.Samples.Features;
using MvvmScarletToolkit.Wpf.Samples.Features.Image;
using Serilog;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Reactive.Concurrency;
using System.Runtime.Versioning;
using System.Windows;
using System.Windows.Media.Imaging;

namespace MvvmScarletToolkit.Wpf.Samples
{
    [SupportedOSPlatform("windows7.0")]
    public partial class App : Application
    {
        private static readonly TimeSpan ShutDownTimeout = TimeSpan.FromSeconds(5);

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

            // NavigationViewModel creates the other scenes itself, only what is resolved from the container is registered here
            serviceCollection.AddSingleton<MainWindow>();
            serviceCollection.AddSingleton<NavigationViewModel>();
            serviceCollection.AddSingleton<ImageViewModelProvider>();
            serviceCollection.AddSingleton<ProcessingImagesViewModel>();
            serviceCollection.AddSingleton<LocalizationsViewModel>();
            serviceCollection.AddSingleton<IFileSystemViewModelFactory, FileSystemViewModelFactory>();
            serviceCollection.AddSingleton(FileSystemOptionsViewModel.Default);

            serviceCollection.AddSingleton<IScheduler>(provider => new SynchronizationContextScheduler(provider.GetRequiredService<SynchronizationContext>()));
            serviceCollection.AddSingleton<ILocalizationProvider, ScarletLocalizationProvider>();
            serviceCollection.AddSingleton(_ => SynchronizationContext.Current!);
            serviceCollection.AddSingleton(ScarletCommandBuilder.Default);
            serviceCollection.AddSingleton(tracker);
            serviceCollection.AddSingleton<HttpClient>();
            serviceCollection.AddSingleton<RecyclableMemoryStreamManager>();
            serviceCollection.AddSingleton(environmentInformationProvider);
            serviceCollection.AddLogging(builder =>
                builder.AddSerilog(new LoggerConfiguration()
                    .MinimumLevel.Verbose()
                    // every image logs several lines per load, too much for the debug output window
                    .MinimumLevel.Override("MvvmScarletToolkit.ImageLoading", Serilog.Events.LogEventLevel.Information)
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
            // limits concurrent decoding in MagickImageFactory, when that is registered instead of WpfImageFactory
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

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // the image list from picsum.photos is fetched by the image scene when it loads, so a slow or missing network does not delay the window
            var window = Ioc.Default.GetRequiredService<MainWindow>();
            window.Show();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            // images are handed out before they are written to the disk caches, keep what is still pending.
            // Blocking is fine, the image service does not need the UI thread to finish
            WaitForShutDownStep(Ioc.Default.GetRequiredService<ImageService<BitmapSource>>().FlushAsync());

            // raises Exit, which starts unloading the registered viewmodels with a synchronous dispatcher, so waiting here can't deadlock.
            // OnExit must not be async: the process ends as soon as it returns, continuations after an await would never run
            base.OnExit(e);

            WaitForShutDownStep(ScarletExitService.Default.ShutDown());
            Ioc.Default.GetRequiredService<Tracker>().PersistAll();
        }

        private static void WaitForShutDownStep(Task task)
        {
            try
            {
                task.Wait(ShutDownTimeout);
            }
            catch (AggregateException ex)
            {
                // a failing step must not prevent the remaining ones
                Trace.TraceError("Shutting down failed: {0}", ex);
            }
        }
    }
}
