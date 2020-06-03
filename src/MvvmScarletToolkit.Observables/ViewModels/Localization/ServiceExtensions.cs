using MvvmScarletToolkit.Abstractions;
using MvvmScarletToolkit.Observables;
using System.ComponentModel;

namespace MvvmScarletToolkit
{
    public static class ServiceExtensions
    {
        public static ILocalizationViewModel CreateViewModel(this ILocalizationService localizationService, IScarletEventManager<INotifyPropertyChanged, PropertyChangedEventArgs> weakEventManager, string key)
        {
            return new LocalizationViewModel(weakEventManager, localizationService, key);
        }
    }
}
