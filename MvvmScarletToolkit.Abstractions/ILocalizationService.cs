using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;

namespace MvvmScarletToolkit.Abstractions
{
    public interface ILocalizationService : INotifyPropertyChanged
    {
        CultureInfo CurrentLanguage { get; set; }

        IEnumerable<CultureInfo> Languages { get; }

        string Translate(string key);
    }
}
