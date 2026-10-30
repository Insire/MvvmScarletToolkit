# MvvmScarletToolkit.ImageLoading

## Goals

MvvmScarletToolkit.ImageLoading is part of the MvvmScarletToolkit framework, containing services and platform abstractions for loading and caching images from remote resources like the web.

## Features

- **Two levels of caching:** raw data and decoded images are each cached in memory and on disk. Every cache can be enabled on its own.
- **Resizing:** images are decoded for the requested size, with a configurable default.
- **Works with virtualized lists:** cancelled loads return `null` instead of throwing, because cancelling is routine for items that scroll out of view.
- **Background disk writes:** images are handed out before they are written to disk, and `MaxConcurrentDiskCacheWrites` limits how many writes run at once. Call `FlushAsync` before the application exits to keep pending writes.
- **Failed load protection:** a failed resource is not requested again for `FailedLoadRetryDelayInSeconds`.
- **Download limits:** `MaxDownloadSizeInBytes` and `RequestTimeout` protect against oversized or slow resources.
- **Cache cleanup:** the disk caches can remove old files on startup through `MaxFileAgeInDays`, or clear themselves with `ClearCacheDirectoryOnInit`.
- **UI framework independent:** an `IImageFactory<TImage>` creates the images. `WpfImageFactory` and `AvaloniaImageFactory` are part of the WPF and Avalonia packages.

## Contents

|class|summary|
|---|---|
|[``ImageService<TImage>``](ImageService.cs)|Provides images by checking the caches, before fetching and decoding them.|
|[``ImageDataProvider``](ImageDataProvider.cs)|Fetches the content of a uri.|
|[``ImageMemoryCache<TImage>``](Caches/ImageMemoryCache.cs)|Memory cache for decoded images.|
|[``ImageFilesystemCache<TImage>``](Caches/ImageFilesystemCache.cs)|Disk cache for encoded images.|
|[``ImageDataMemoryCache``](Caches/Data/ImageDataMemoryCache.cs)|Memory cache for raw image data.|
|[``ImageDataFileystemCache``](Caches/Data/ImageDataFileystemCache.cs)|Disk cache for raw image data.|

### Options

Each service reads its settings from an options class. The caches are disabled by default, and the disk caches need `CreateFolder` to create their directory.

|class|settings|
|---|---|
|[``ImageServiceOptions``](ImageServiceOptions.cs)|``DefaultHeight``, ``DefaultWidth``, ``FailedLoadRetryDelayInSeconds``, ``MaxConcurrentDiskCacheWrites``|
|[``ImageDataProviderOptions``](ImageDataProviderOptions.cs)|``MaxDownloadSizeInBytes``, ``RequestTimeout``|
|[``ImageMemoryCacheOptions``](Caches/ImageMemoryCacheOptions.cs)|``IsEnabled``, ``AbsoluteExpirationOffsetInSeconds``|
|[``ImageDataMemoryCacheOptions``](Caches/Data/ImageDataMemoryCacheOptions.cs)|``IsEnabled``, ``AbsoluteExpirationOffsetInSeconds``|
|[``ImageFilesystemCacheOptions``](Caches/ImageFilesystemCacheOptions.cs)|``CacheDirectoryPath``, ``ClearCacheDirectoryOnInit``, ``CreateFolder``, ``IsEnabled``, ``MaxFileAgeInDays``|
|[``ImageDataFileystemCacheOptions``](Caches/Data/ImageDataFileystemCacheOptions.cs)|``CacheDirectoryPath``, ``ClearCacheDirectoryOnInit``, ``CreateFolder``, ``IsEnabled``, ``MaxFileAgeInDays``|

## Usage

The example uses WPF. For Avalonia, replace `BitmapSource` with `Bitmap` and `WpfImageFactory` with `AvaloniaImageFactory`.

```cs
services.AddLogging();
services.AddMemoryCache();
services.AddSingleton<HttpClient>();
services.AddSingleton<RecyclableMemoryStreamManager>();

// limits how many images are decoded at the same time
services.AddSingleton(new SemaphoreSlim(Environment.ProcessorCount));

services.AddSingleton(new ImageServiceOptions());
services.AddSingleton(new ImageDataProviderOptions());
services.AddSingleton(new ImageMemoryCacheOptions { IsEnabled = true });
services.AddSingleton(new ImageDataMemoryCacheOptions { IsEnabled = true });
services.AddSingleton(new ImageFilesystemCacheOptions());
services.AddSingleton(new ImageDataFileystemCacheOptions());

services.AddSingleton<IImageDataProvider, ImageDataProvider>();
services.AddSingleton<IImageDataMemoryCache, ImageDataMemoryCache>();
services.AddSingleton<IImageDataFileystemCache, ImageDataFileystemCache>();
services.AddSingleton<IImageMemoryCache<BitmapSource>, ImageMemoryCache<BitmapSource>>();
services.AddSingleton<IImageFilesystemCache<BitmapSource>, ImageFilesystemCache<BitmapSource>>();
services.AddSingleton<IImageFactory<BitmapSource>, WpfImageFactory>();
services.AddSingleton<IImageService<BitmapSource>, ImageService<BitmapSource>>();
services.AddSingleton(provider => new Lazy<IImageService<BitmapSource>>(() => provider.GetRequiredService<IImageService<BitmapSource>>()));
```

Assign the service to the behavior once at startup:

```cs
AsyncImageLoadingBehavior.Loader = serviceProvider.GetRequiredService<Lazy<IImageService<BitmapSource>>>();
```

```xml
<Image>
    <i:Interaction.Behaviors>
        <mvvm:AsyncImageLoadingBehavior Source="{Binding Url}" Width="300" Height="300" />
    </i:Interaction.Behaviors>
</Image>
```

To use the service without a behavior, call `ProvideImageAsync`. The callback reports whether the image is loading from a slow source, which allows showing a placeholder:

```cs
var image = await imageService.ProvideImageAsync(uri, new ImageSize(300, 300), isLoadingSlowly => Task.CompletedTask, token);
```

The result is `null` when the image could not be loaded or the load was cancelled.
