using System.Collections.Generic;
using System.Globalization;

namespace MvvmScarletToolkit
{
    public interface ILocalizationProvider
    {
        /// <summary>
        /// Translates the key into a localized value
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns></returns>
        string Translate(string key, CultureInfo culture);

        /// <summary>
        /// Gets the available languages.
        /// </summary>
        /// <value>The available languages.</value>
        IEnumerable<CultureInfo> Languages { get; }
    }
}
