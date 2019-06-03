using MvvmScarletToolkit.Abstractions;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace MvvmScarletToolkit.Observables
{
    public class LocalizationsViewModel : BusinessViewModelListBase<LocalizationViewModel>, ILocalizationService
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

        public LocalizationsViewModel(ICommandBuilder commandBuilder)
            : base(commandBuilder)
        {
        }

        public LocalizationsViewModel(ICommandBuilder commandBuilder, ILocalizationProvider provider)
            : base(commandBuilder)
        {
            TranslationProvider = provider ?? throw new ArgumentNullException(nameof(provider));
        }

        public string Translate(string key)
        {
            if (TranslationProvider != null)
            {
                if (Thread.CurrentThread.CurrentUICulture != CurrentLanguage)
                    Thread.CurrentThread.CurrentUICulture = CurrentLanguage;

                var translatedValue = TranslationProvider.Translate(key);
                if (!string.IsNullOrEmpty(translatedValue))
                {
                    return translatedValue;
                }
            }

            return $"!{key}!";
        }

        public LocalizationViewModel Add(string key)
        {
            var viewModel = new LocalizationViewModel(this, key);
            _items.Add(viewModel);

            return viewModel;
        }

        protected override Task RefreshInternal(CancellationToken token)
        {
            CurrentLanguage = Thread.CurrentThread.CurrentUICulture;
            return Dispatcher.Invoke(() => OnPropertyChanged(nameof(Languages)), token);
        }
    }
}
