namespace MvvmScarletToolkit.Wpf
{
    internal sealed class HTMLTagInfo
    {
        public string Html { get; }
        public HTMLFlag Flags { get; }
        public short TagLevel { get; }

        public HTMLTagInfo(string html, HTMLFlag flags, short tagLevel)
        {
            Html = html;
            Flags = flags;
            TagLevel = tagLevel;
        }
    }
}
