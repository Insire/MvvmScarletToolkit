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

        private bool _disposed;

        public LocalizationViewModel(in IScarletEventManager<INotifyPropertyChanged, PropertyChangedEventArgs> weakEventManager, in ILocalizationService service, in string key)
            : this(weakEventManager, service, key, false)
        {
        }

        public LocalizationViewModel(in IScarletEventManager<INotifyPropertyChanged, PropertyChangedEventArgs> weakEventManager, in ILocalizationService service, in string key, in bool toUpper)
        {
            _weakEventManager = weakEventManager ?? throw new ArgumentNullException(nameof(weakEventManager));
            _service = service ?? throw new ArgumentNullException(nameof(service));
            _key = key ?? throw new ArgumentNullException(nameof(key));
            _toUpper = toUpper;

            _weakEventManager.AddHandler(_service, nameof(ILocalizationService.PropertyChanged), ValueChanged);
        }

        public object Value => GetValue();

        private void ValueChanged(object sender, PropertyChangedEventArgs e)
        {
            OnPropertyChanged(nameof(Value));
        }

        private string GetValue()
        {
            if (_disposed)
            {
                return string.Empty;
            }

            if (_toUpper)
            {
                return _service?.Translate(_key)?.ToUpperInvariant() ?? _key;
            }

            return _service?.Translate(_key) ?? _key;
        }

        public void Dispose()
        {
            Dispose(true);
        }

        private void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                _weakEventManager.RemoveHandler(_service, nameof(ILocalizationService.PropertyChanged), ValueChanged);
            }

            _disposed = true;
        }
    }
}
