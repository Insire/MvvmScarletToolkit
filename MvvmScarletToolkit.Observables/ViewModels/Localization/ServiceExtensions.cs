using MvvmScarletToolkit.Abstractions;

namespace MvvmScarletToolkit.Observables
{
    public static class ServiceExtensions
    {
        public static ILocalizationViewModel CreateViewModel(this ILocalizationService localizationService, string key)
        {
            return new LocalizationViewModel(localizationService, key);
        }
    }
}
