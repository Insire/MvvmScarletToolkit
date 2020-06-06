namespace MvvmScarletToolkit.Wpf
{
    internal sealed class HTMLTagInfo
    {
        public string Html { get; }
        public HTMLFlag Flags { get; }
        public short TagLevel { get; }

        public HTMLTagInfo(string aHtml, HTMLFlag aFlags, short aTagLevel)
        {
            Html = aHtml;
            Flags = aFlags;
            TagLevel = aTagLevel;
        }
    }
}
