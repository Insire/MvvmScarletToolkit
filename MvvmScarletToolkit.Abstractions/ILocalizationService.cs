using System.ComponentModel;
using System.Globalization;

namespace MvvmScarletToolkit.Abstractions
{
    public interface ILocalizationService : ILocalizationProvider, INotifyPropertyChanged
    {
        CultureInfo CurrentLanguage { get; set; }
    }
}
