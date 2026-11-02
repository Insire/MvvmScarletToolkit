using System.Globalization;

namespace MvvmScarletToolkit
{
    public sealed class ScarletLocalizationProvider : ILocalizationProvider
    {
        public IEnumerable<CultureInfo> Languages { get; }

        public ScarletLocalizationProvider()
        {
            Languages = [];
        }

        public string Translate(string key, CultureInfo culture)
        {
            return key;
        }
    }
}
