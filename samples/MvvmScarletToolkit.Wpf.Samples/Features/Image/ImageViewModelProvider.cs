using JetBrains.Annotations;
using Microsoft.Extensions.Logging;
using MoreLinq;
using MvvmScarletToolkit.Core.Samples.Features;
using System.IO;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace MvvmScarletToolkit.Wpf.Samples.Features.Image
{
    public sealed class ImageViewModelProvider
    {
        [UsedImplicitly]
        private sealed class ImageDto
        {
            [JsonPropertyName("id")]
            public long Id { get; set; }

            [JsonPropertyName("author")]
            public string Author { get; set; } = default!;

            [JsonPropertyName("width")]
            public long Width { get; set; }

            [JsonPropertyName("height")]
            public long Height { get; set; }

            [JsonPropertyName("url")]
            public Uri Url { get; set; } = default!;

            [JsonPropertyName("download_url")]
            public Uri DownloadUrl { get; set; } = default!;
        }

        private readonly string[] _urls = new string[]
        {
            "https://w.wallhaven.cc/full/o5/wallhaven-o58767.jpg",
            "https://w.wallhaven.cc/full/1p/wallhaven-1pyl9g.jpg",
            "https://w.wallhaven.cc/full/jx/wallhaven-jx7y95.jpg",
            "https://w.wallhaven.cc/full/9d/wallhaven-9dpejk.jpg",
            "https://w.wallhaven.cc/full/6d/wallhaven-6d6pvl.jpg",
        };

        private static readonly TimeSpan InitializationTimeout = TimeSpan.FromSeconds(10);

        private readonly HashSet<string> _randomWebUrls;
        private readonly IScarletCommandBuilder _commandBuilder;
        private readonly EnvironmentInformationProvider _environmentInformationProvider;
        private readonly HttpClient _httpClient;
        private readonly ILogger<ImageViewModelProvider> _logger;
        private readonly Lock _initializationLock;
        private Task<bool>? _initialization;
        private int _index;

        public ImageViewModelProvider(
            IScarletCommandBuilder commandBuilder,
            EnvironmentInformationProvider environmentInformationProvider,
            HttpClient httpClient,
            ILogger<ImageViewModelProvider> logger)
        {
            _commandBuilder = commandBuilder ?? throw new ArgumentNullException(nameof(commandBuilder));
            _environmentInformationProvider = environmentInformationProvider;
            _httpClient = httpClient;
            _logger = logger;
            _randomWebUrls = new HashSet<string>();
            _initializationLock = new Lock();
        }

        /// <summary>
        /// Fetches the list of random images once. A failed attempt is logged, and repeated on the next call.
        /// </summary>
        public Task Initialize(CancellationToken cancellationToken)
        {
            Task<bool> initialization;
            lock (_initializationLock)
            {
                if (_initialization is null || (_initialization.IsCompleted && !_initialization.Result))
                {
                    _initialization = InitializeInternal();
                }

                initialization = _initialization;
            }

            // the fetch is shared, cancelling one caller must not cancel it for the others
            return initialization.WaitAsync(cancellationToken);
        }

        private async Task<bool> InitializeInternal()
        {
            try
            {
                using var timeout = new CancellationTokenSource(InitializationTimeout);
                var models = await _httpClient.GetFromJsonAsync<List<ImageDto>>("https://picsum.photos/v2/list", timeout.Token).ConfigureAwait(false);
                if (models != null)
                {
                    _randomWebUrls.AddRange(models.Select(p => p.DownloadUrl.ToString()));
                }

                return true;
            }
            catch (Exception ex) when (ex is HttpRequestException or OperationCanceledException or JsonException or NotSupportedException)
            {
                _logger.LogWarning(ex, "Fetching the image list from picsum.photos failed, continuing without random images");
                return false;
            }
        }

        public IEnumerable<ImageViewModel> GetImages(SourceImagesViewModel source, TargetImagesViewModel target)
        {
            // Sequence restarts with every refresh
            Interlocked.Exchange(ref _index, 0);

            return GetLocalImages(source, target).Interleave(GetWebImages(source, target), GetRandomWebImages(source, target));
        }

        private IEnumerable<ImageViewModel> GetLocalImages(SourceImagesViewModel source, TargetImagesViewModel target)
        {
            var directoryPath = _environmentInformationProvider.GetResourceFolderPath();

            var files = Directory.GetFiles(directoryPath, searchPattern: "*.jpg", SearchOption.TopDirectoryOnly);
            if (files.Length == 0)
            {
                var assembly = typeof(ImageViewModelProvider).Assembly;
                foreach (var name in assembly.GetManifestResourceNames().Where(p => p.EndsWith(".jpg")))
                {
                    using var resourceStream = assembly.GetManifestResourceStream(name);
                    if (resourceStream == null)
                    {
                        continue;
                    }

                    var filePath = Path.Combine(directoryPath, name);
                    using var fileStream = File.OpenWrite(filePath);
                    resourceStream.CopyTo(fileStream);
                }
            }

            foreach (var filePath in Directory.GetFiles(directoryPath, searchPattern: "*.jpg", SearchOption.TopDirectoryOnly))
            {
                var index = Interlocked.Increment(ref _index);

                yield return new ImageViewModel(_commandBuilder, target, source)
                {
                    IsSelected = index == 1,
                    DisplayName = Path.GetFileName(filePath),
                    Path = filePath,
                    Sequence = index,
                };
            }
        }

        private IEnumerable<ImageViewModel> GetWebImages(SourceImagesViewModel source, TargetImagesViewModel target)
        {
            for (var i = 0; i < _urls.Length; i++)
            {
                var index = Interlocked.Increment(ref _index);

                yield return new ImageViewModel(_commandBuilder, target, source)
                {
                    IsSelected = index == 1,
                    DisplayName = $"Image {index}",
                    Path = _urls[i],
                    Sequence = index,
                };
            }
        }

        private IEnumerable<ImageViewModel> GetRandomWebImages(SourceImagesViewModel source, TargetImagesViewModel target)
        {
            var randomWebUrls = _randomWebUrls.ToArray();

            for (var i = 0; i < randomWebUrls.Length; i++)
            {
                var index = Interlocked.Increment(ref _index);

                yield return new ImageViewModel(_commandBuilder, target, source)
                {
                    IsSelected = index == 1,
                    DisplayName = $"Image {index}",
                    Path = randomWebUrls[i],
                    Sequence = index,
                };
            }
        }
    }
}
