using MoreLinq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace MvvmScarletToolkit.Wpf.Samples.Features.Image
{
    public sealed class ImageViewModelProvider
    {
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

        private readonly HashSet<string> _randomWebUrls;
        private readonly IScarletCommandBuilder _commandBuilder;
        private readonly EnvironmentInformationProvider _environmentInformationProvider;
        private readonly HttpClient _httpClient;
        private int _index;

        public ImageViewModelProvider(IScarletCommandBuilder commandBuilder, EnvironmentInformationProvider environmentInformationProvider, HttpClient httpClient)
        {
            _commandBuilder = commandBuilder ?? throw new ArgumentNullException(nameof(commandBuilder));
            _environmentInformationProvider = environmentInformationProvider;
            _httpClient = httpClient;
            _randomWebUrls = new HashSet<string>();
        }

        public async Task Initialize(CancellationToken cancellationToken)
        {
            var models = await _httpClient.GetFromJsonAsync<List<ImageDto>>("https://picsum.photos/v2/list", cancellationToken);
            if (models != null)
            {
                _randomWebUrls.AddRange(models.Select(p => p.DownloadUrl.ToString()));
            }
        }

        public IEnumerable<ImageViewModel> GetImages()
        {
            return GetLocalImages().Interleave(GetWebImages(), GetRandomWebImages());
        }

        private IEnumerable<ImageViewModel> GetLocalImages()
        {
            var directoryPath = _environmentInformationProvider.GetResourceFolderPath();

            foreach (var filePath in Directory.GetFiles(directoryPath, searchPattern: "*.jpg", SearchOption.TopDirectoryOnly))
            {
                yield return new ImageViewModel(_commandBuilder)
                {
                    IsSelected = _index == 0,
                    DisplayName = Path.GetFileName(filePath),
                    Path = filePath,
                    Sequence = _index,
                };

                _index++;
            }
        }

        private IEnumerable<ImageViewModel> GetWebImages()
        {
            for (var i = 0; i < _urls.Length; i++)
            {
                yield return new ImageViewModel(_commandBuilder)
                {
                    IsSelected = _index == 0,
                    DisplayName = $"Image {i}",
                    Path = _urls[i],
                    Sequence = _index,
                };
            }
        }

        private IEnumerable<ImageViewModel> GetRandomWebImages()
        {
            var randomWebUrls = _randomWebUrls.ToArray();

            for (var i = 0; i < randomWebUrls.Length; i++)
            {
                yield return new ImageViewModel(_commandBuilder)
                {
                    IsSelected = _index == 0,
                    DisplayName = $"Image {i}",
                    Path = randomWebUrls[i],
                    Sequence = _index,
                };
            }
        }
    }
}
