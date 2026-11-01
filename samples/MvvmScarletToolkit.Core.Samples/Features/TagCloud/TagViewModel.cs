namespace MvvmScarletToolkit.Core.Samples.Features.TagCloud
{
    public sealed class TagViewModel
    {
        public string Name { get; }

        /// <summary>
        /// How often this tag is used, popular tags are displayed larger
        /// </summary>
        public int Usages { get; }

        public double FontSize { get; }

        public TagViewModel(string name, int usages, double fontSize)
        {
            Name = name;
            Usages = usages;
            FontSize = fontSize;
        }
    }
}
