using JetBrains.Annotations;
using System.Text.Json.Serialization;

namespace MvvmScarletToolkit.Core.Samples.Features.Image
{
    /// <summary>
    /// An entry of the image list from https://picsum.photos/v2/list.
    /// </summary>
    [UsedImplicitly]
    internal sealed class PicsumImage
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = default!;

        [JsonPropertyName("author")]
        public string Author { get; set; } = default!;

        [JsonPropertyName("download_url")]
        public Uri DownloadUrl { get; set; } = default!;
    }

    /// <summary>
    /// Source generated serialization for <see cref="PicsumImage"/>, which works with trimming and Native AOT.
    /// </summary>
    [JsonSerializable(typeof(List<PicsumImage>))]
    internal sealed partial class PicsumJsonContext : JsonSerializerContext
    {
    }
}
