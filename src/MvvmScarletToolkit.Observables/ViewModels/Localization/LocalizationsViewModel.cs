using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;

namespace MvvmScarletToolkit.Observables
{
    /// <summary>
    /// Viewmodel providing binding support for current language and support languages of a given <see cref="ILocalizationProvider"/>
    /// </summary>
    public class LocalizationsViewModel : ObservableObject, ILocalizationService
    {
        protected ILocalizationProvider LocalizationProvider { get; }

        private CultureInfo? _currentLanguage;
        public CultureInfo? CurrentLanguage
        {
            get { return _currentLanguage; }
            set
            {
                if (SetProperty(ref _currentLanguage, value))
                {
                    Thread.CurrentThread.CurrentUICulture = value;
                }
            }
        }

        public IEnumerable<CultureInfo> Languages => LocalizationProvider.Languages?.Any() == true
            ? LocalizationProvider.Languages
            : Enumerable.Empty<CultureInfo>();

        public LocalizationsViewModel(ILocalizationProvider provider)
        {
            LocalizationProvider = provider ?? throw new ArgumentNullException(nameof(provider));

            CurrentLanguage = LocalizationProvider.Languages.FirstOrDefault(p => p.LCID == Thread.CurrentThread.CurrentUICulture.LCID)
                              ?? LocalizationProvider.Languages.FirstOrDefault()
                              ?? Thread.CurrentThread.CurrentUICulture;
        }

        public string Translate(string key)
        {
            if (CurrentLanguage == null)
            {
                return $"!{key}!";
            }

            if (Thread.CurrentThread.CurrentUICulture.Equals(CurrentLanguage))
            {
                Thread.CurrentThread.CurrentUICulture = CurrentLanguage;
            }

            var translatedValue = LocalizationProvider.Translate(key, CurrentLanguage);
            return string.IsNullOrEmpty(translatedValue)
                ? $"!{key}!"
                : translatedValue;
        }
    }
}
