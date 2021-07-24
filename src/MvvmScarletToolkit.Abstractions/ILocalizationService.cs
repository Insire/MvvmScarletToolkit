using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;

namespace MvvmScarletToolkit
{
    public interface ILocalizationService : INotifyPropertyChanged
    {
        CultureInfo? CurrentLanguage { get; set; }

        /// <summary>
        /// Translates the key into a localized value
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns></returns>
        string Translate(string key);

        /// <summary>
        /// Gets the available languages.
        /// </summary>
        /// <value>The available languages.</value>
        IEnumerable<CultureInfo> Languages { get; }
    }
}
