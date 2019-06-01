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

        public IEnumerable<CultureInfo> Languages => TranslationProvider is null
            ? Enumerable.Empty<CultureInfo>()
            : TranslationProvider.Languages;

        public LocalizationsViewModel(ILocalizationProvider provider)
        {
            TranslationProvider = provider ?? throw new ArgumentNullException(nameof(provider));
            _currentLanguage = Thread.CurrentThread.CurrentUICulture;
        }

        public string Translate(string key)
        {
            if (TranslationProvider != null)
            {
                if (Thread.CurrentThread.CurrentUICulture != CurrentLanguage)
                    Thread.CurrentThread.CurrentUICulture = CurrentLanguage;

                var translatedValue = TranslationProvider.Translate(key);
                if (!string.IsNullOrEmpty(translatedValue))
                    return translatedValue;
            }

            return $"!{key}!";
        }
    }
}
