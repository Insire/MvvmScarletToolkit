using MvvmScarletToolkit.Abstractions;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace MvvmScarletToolkit
{
    public sealed class ScarletLocalizationProvider : ILocalizationProvider
    {
        public IEnumerable<CultureInfo> Languages { get; }

        public ScarletLocalizationProvider()
        {
            Languages = Enumerable.Empty<CultureInfo>();
        }

        public string Translate(string key, CultureInfo culture)
        {
            return key;
        }
    }
}
