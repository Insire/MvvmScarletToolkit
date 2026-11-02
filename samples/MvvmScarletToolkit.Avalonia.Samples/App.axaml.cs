using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.Media.Imaging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IO;
using MvvmScarletToolkit.Avalonia.Samples.Features;
using MvvmScarletToolkit.Avalonia.Samples.Views;
using MvvmScarletToolkit.Core.Samples.Features;
using MvvmScarletToolkit.Core.Samples.Features.Image;
using MvvmScarletToolkit.ImageLoading;
using MvvmScarletToolkit.ImageLoading.Caches;
using MvvmScarletToolkit.ImageLoading.Caches.Data;
using MvvmScarletToolkit.Observables;
using System.Net.Http;

namespace MvvmScarletToolkit.Avalonia.Samples;

public partial class App : Application
{
    private static readonly TimeSpan ShutDownTimeout = TimeSpan.FromSeconds(5);

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            var services = ConfigureServices();

            AsyncImageLoadingBehavior.Loader = services.GetRequiredService<Lazy<IImageService<Bitmap>>>();

            desktop.MainWindow = new MainWindow { DataContext = services.GetRequiredService<NavigationViewModel>(), };

            // an open toast window must not keep the application alive
            desktop.ShutdownMode = ShutdownMode.OnMainWindowClose;

            // images are handed out before they are written to the disk caches, keep what is still pending
            desktop.Exit += (_, _) => services.GetRequiredService<ImageService<Bitmap>>().FlushAsync().Wait(ShutDownTimeout);
        }

        base.OnFrameworkInitializationCompleted();
    }

    private static ServiceProvider ConfigureServices()
    {
        var serviceCollection = new ServiceCollection();

        var environmentInformationProvider = new EnvironmentInformationProvider();

        // NavigationViewModel creates the other scenes itself, only what is resolved from the container is registered here
        serviceCollection.AddSingleton<NavigationViewModel>();
        serviceCollection.AddSingleton<ImageViewModelProvider>();
        serviceCollection.AddSingleton<ProcessingImagesViewModel>();
        serviceCollection.AddSingleton<LocalizationsViewModel>();

        serviceCollection.AddSingleton<ILocalizationProvider, ScarletLocalizationProvider>();
        serviceCollection.AddSingleton(_ => SynchronizationContext.Current!);
        serviceCollection.AddSingleton(ScarletCommandBuilder.Default);
        serviceCollection.AddSingleton<HttpClient>();
        serviceCollection.AddSingleton<RecyclableMemoryStreamManager>();
        serviceCollection.AddSingleton(environmentInformationProvider);
        serviceCollection.AddLogging();

        serviceCollection.AddSingleton<ImageService<Bitmap>>();
        serviceCollection.AddSingleton<IImageDataProvider, ImageDataProvider>();
        serviceCollection.AddSingleton<IImageFactory<Bitmap>, AvaloniaImageFactory>();
        serviceCollection.AddSingleton<IImageDataFileystemCache, ImageDataFileystemCache>();
        serviceCollection.AddSingleton<IImageFilesystemCache<Bitmap>, ImageFilesystemCache<Bitmap>>();
        serviceCollection.AddSingleton<IImageDataMemoryCache, ImageDataMemoryCache>();
        serviceCollection.AddSingleton<IImageMemoryCache<Bitmap>, ImageMemoryCache<Bitmap>>();
        // limits concurrent decoding in AvaloniaImageFactory
        serviceCollection.AddSingleton(new SemaphoreSlim(Environment.ProcessorCount));
        serviceCollection.AddSingleton(new ImageDataFileystemCacheOptions() { CacheDirectoryPath = environmentInformationProvider.GetRawImagesFolderPath(), CreateFolder = true, IsEnabled = true });
        serviceCollection.AddSingleton(new ImageFilesystemCacheOptions() { IsEnabled = true, CacheDirectoryPath = environmentInformationProvider.GetEncodedImagesFolderPath(), CreateFolder = true });
        serviceCollection.AddSingleton(new ImageDataMemoryCacheOptions() { IsEnabled = true });
        serviceCollection.AddSingleton(new ImageMemoryCacheOptions() { IsEnabled = true });
        serviceCollection.AddSingleton(new ImageServiceOptions() { DefaultHeight = 300, DefaultWidth = 300 });
        serviceCollection.AddSingleton(provider => new Lazy<IImageService<Bitmap>>(() => provider.GetRequiredService<ImageService<Bitmap>>()));
        serviceCollection.AddMemoryCache();

        return serviceCollection.BuildServiceProvider();
    }
}
