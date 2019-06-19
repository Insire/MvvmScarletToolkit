using MvvmScarletToolkit.Abstractions;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;

namespace MvvmScarletToolkit.Observables
{
    public class LocalizationsViewModel : ObservableObject, ILocalizationService
    {
        protected readonly ILocalizationProvider TranslationProvider;

        private CultureInfo _currentLanguage;
        public CultureInfo CurrentLanguage
        {
            get { return _currentLanguage; }
            set { SetValue(ref _currentLanguage, value, () => Thread.CurrentThread.CurrentUICulture = value); }
        }

        public IEnumerable<CultureInfo> Languages => TranslationProvider.Languages?.Any() == true
            ? TranslationProvider.Languages
            : Enumerable.Empty<CultureInfo>();

        public LocalizationsViewModel(ILocalizationProvider provider)
        {
            TranslationProvider = provider ?? throw new ArgumentNullException(nameof(provider));

            CurrentLanguage = Thread.CurrentThread.CurrentUICulture;
        }

        public string Translate(string key)
        {
            if (TranslationProvider != null && CurrentLanguage != null)
            {
                if (Thread.CurrentThread.CurrentUICulture != CurrentLanguage)
                    Thread.CurrentThread.CurrentUICulture = CurrentLanguage;

                var translatedValue = TranslationProvider.Translate(key, CurrentLanguage);
                if (!string.IsNullOrEmpty(translatedValue))
                {
                    return translatedValue;
                }
            }

            return $"!{key}!";
        }
    }
}
