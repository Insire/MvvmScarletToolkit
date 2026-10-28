using CommunityToolkit.Mvvm.ComponentModel;
using JetBrains.Annotations;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json.Serialization;

namespace MvvmScarletToolkit.Wpf.Samples.Features.Image
{
    /// <summary>
    /// Lots of images in a virtualized list, to observe how images get loaded, cancelled and served from cache while scrolling.
    /// </summary>
    public sealed partial class VirtualizedImagesViewModel : BusinessSourceListViewModelBase<VirtualizedImageViewModel>
    {
        [UsedImplicitly]
        private sealed class ImageDto
        {
            [JsonPropertyName("id")]
            public string Id { get; set; } = default!;

            [JsonPropertyName("author")]
            public string Author { get; set; } = default!;
        }

        private const int Pages = 5;
        private const int PageSize = 100;
        private const int BrokenImageInterval = 25;

        public const int ImageWidth = 300;
        public const int ImageHeight = 200;

        private readonly HttpClient _httpClient;

        [ObservableProperty]
        public partial int RealizedCount { get; private set; }

        [ObservableProperty]
        public partial int LoadingCount { get; private set; }

        [ObservableProperty]
        public partial int ShownCount { get; private set; }

        [ObservableProperty]
        public partial int UnrealizedCount { get; private set; }

        [ObservableProperty]
        public partial int FailedCount { get; private set; }

        [ObservableProperty]
        public partial string? Error { get; private set; }

        public VirtualizedImagesViewModel(IScarletCommandBuilder commandBuilder, SynchronizationContext synchronizationContext, HttpClient httpClient)
            : base(commandBuilder, synchronizationContext, vm => vm.Id)
        {
            _httpClient = httpClient;
        }

        protected override async Task RefreshInternal(CancellationToken token)
        {
            Error = null;

            try
            {
                var pages = await Task.WhenAll(Enumerable.Range(1, Pages)
                    .Select(page => _httpClient.GetFromJsonAsync<List<ImageDto>>($"https://picsum.photos/v2/list?page={page}&limit={PageSize}", token)));

                var viewModels = pages
                    .SelectMany(page => page ?? [])
                    .Select((dto, index) => index % BrokenImageInterval == BrokenImageInterval - 1
                        // ids that do not exist, picsum answers with a 404, to show failed loads
                        ? new VirtualizedImageViewModel(CommandBuilder, $"{dto.Id}-broken", "(broken on purpose)", new Uri($"https://picsum.photos/id/{100000 + index}/{ImageWidth}/{ImageHeight}"), OnStateChanged)
                        : new VirtualizedImageViewModel(CommandBuilder, dto.Id, dto.Author, new Uri($"https://picsum.photos/id/{dto.Id}/{ImageWidth}/{ImageHeight}"), OnStateChanged))
                    .ToArray();

                await AddRange(viewModels, token);
            }
            catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
            {
                Error = $"Fetching the image list from picsum.photos failed: {ex.Message}";
            }

            UpdateCounts();
        }

        private void OnStateChanged(VirtualizedImageViewModel viewModel)
        {
            UpdateCounts();
        }

        private void UpdateCounts()
        {
            int realized = 0, loading = 0, shown = 0, unrealized = 0, failed = 0;

            foreach (var item in Items)
            {
                if (item.IsRealized)
                {
                    realized++;
                }

                switch (item.State)
                {
                    case VirtualizedImageState.Failed:
                        failed++;
                        break;

                    case VirtualizedImageState.Loading:
                        loading++;
                        break;

                    case VirtualizedImageState.Shown:
                        shown++;
                        break;

                    case VirtualizedImageState.Unrealized:
                        unrealized++;
                        break;
                }
            }

            RealizedCount = realized;
            LoadingCount = loading;
            ShownCount = shown;
            UnrealizedCount = unrealized;
            FailedCount = failed;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                Items.Dispose();
            }

            base.Dispose(disposing);
        }
    }
}
