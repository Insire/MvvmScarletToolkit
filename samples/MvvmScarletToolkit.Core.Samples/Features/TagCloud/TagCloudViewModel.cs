using CommunityToolkit.Mvvm.ComponentModel;

namespace MvvmScarletToolkit.Core.Samples.Features.TagCloud
{
    /// <summary>
    /// A lot of tags that all differ in size, which is what a wrapping panel handles and a panel with fixed tiles can't.
    /// Generated from a fixed seed, so that this works offline and looks the same on every start.
    /// </summary>
    public sealed partial class TagCloudViewModel : ObservableObject
    {
        private const int TagCount = 100_000;

        private static readonly string[] Syllables =
        [
            "ka", "lo", "mi", "ren", "sa", "to", "vu", "zen", "bri", "cor", "dal", "fen", "gor", "hal", "jun", "kel", "lum", "mor", "nor", "pax",
        ];

        private readonly IReadOnlyList<TagViewModel> _tags;

        [ObservableProperty]
        public partial IReadOnlyList<TagViewModel> Tags { get; private set; }

        [ObservableProperty]
        public partial string FilterText { get; set; }

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(Selection))]
        public partial TagViewModel? SelectedTag { get; set; }

        public string Selection => SelectedTag is null ? "Select a tag" : $"{SelectedTag.Name} is used {SelectedTag.Usages:N0} times";

        public TagCloudViewModel()
        {
            _tags = CreateTags();
            Tags = _tags;
            FilterText = string.Empty;
        }

        partial void OnFilterTextChanged(string value)
        {
            Tags = string.IsNullOrWhiteSpace(value)
                ? _tags
                : _tags.Where(p => p.Name.Contains(value, StringComparison.OrdinalIgnoreCase)).ToArray();
        }

        private static TagViewModel[] CreateTags()
        {
            var random = new Random(1337);
            var tags = new TagViewModel[TagCount];

            for (var i = 0; i < tags.Length; i++)
            {
                var name = string.Concat(Enumerable.Range(0, random.Next(1, 5)).Select(_ => Syllables[random.Next(Syllables.Length)]));

                // most tags are rare, few are popular
                var popularity = Math.Pow(random.NextDouble(), 4);

                tags[i] = new TagViewModel($"{name}{i}", (int)(popularity * 10_000) + 1, 12 + (popularity * 24));
            }

            return tags;
        }
    }
}
