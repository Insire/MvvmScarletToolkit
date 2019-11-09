using MvvmScarletToolkit.Abstractions;
using System;
using System.ComponentModel;

namespace MvvmScarletToolkit.Observables
{
    /// <summary>
    /// Provides binding support for a localized string
    /// </summary>
    public sealed class LocalizationViewModel : ObservableObject, IDisposable, ILocalizationViewModel
    {
        private readonly string _key;
        private readonly bool _toUpper;
        private readonly ILocalizationService _service;
        private readonly IScarletEventManager<INotifyPropertyChanged, PropertyChangedEventArgs> _weakEventManager;

        public LocalizationViewModel(IScarletEventManager<INotifyPropertyChanged, PropertyChangedEventArgs> weakEventManager, ILocalizationService service, string key, bool toUpper)
        {
            _weakEventManager = weakEventManager ?? throw new ArgumentNullException(nameof(weakEventManager));
            _service = service ?? throw new ArgumentNullException(nameof(service));
            _key = key ?? throw new ArgumentNullException(nameof(key));
            _toUpper = toUpper;

            _weakEventManager.AddHandler(_service, "PropertyChanged", ValueChanged);
        }

        public LocalizationViewModel(IScarletEventManager<INotifyPropertyChanged, PropertyChangedEventArgs> weakEventManager, ILocalizationService service, string key)
            : this(weakEventManager, service, key, false)
        {
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (disposing)
            {
                _weakEventManager.RemoveHandler(_service, "PropertyChanged", ValueChanged);
            }
        }

        public object Value => GetValue();

        private void ValueChanged(object sender, PropertyChangedEventArgs e)
        {
            OnPropertyChanged(nameof(Value));
        }

        private string GetValue()
        {
            if (_toUpper)
            {
                return _service?.Translate(_key).ToUpperInvariant();
            }

            return _service?.Translate(_key);
        }
    }
}
