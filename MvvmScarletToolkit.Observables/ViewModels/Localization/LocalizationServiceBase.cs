using MvvmScarletToolkit.Abstractions;
using MvvmScarletToolkit.Observables;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace MvvmScarletToolkit.Observables
{
    public abstract class LocalizationServiceBase : ObservableObject, ILocalizationService
    {
        public ITranslationProvider TranslationProvider { get; }

        private CultureInfo _currentLanguage;
        public CultureInfo CurrentLanguage
        {
            get { return _currentLanguage; }
            set { SetValue(ref _currentLanguage, value, () => Thread.CurrentThread.CurrentUICulture = value); }
        }

        public IEnumerable<CultureInfo> Languages => TranslationProvider is null ? Enumerable.Empty<CultureInfo>() : TranslationProvider.Languages;

        protected LocalizationServiceBase(ITranslationProvider provider)
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

        public abstract Task Save();

        public abstract Task Load();
    }
}
