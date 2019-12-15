using MvvmScarletToolkit.Abstractions;
using System;

namespace MvvmScarletToolkit.Observables
{
    public class Scene : ObservableObject
    {
        private object _content;
        public object Content
        {
            get { return _content; }
            set { SetValue(ref _content, value); }
        }

        private bool _isSelected;
        public bool IsSelected
        {
            get { return _isSelected; }
            set { SetValue(ref _isSelected, value); }
        }

        public int _sequence;
        public int Sequence
        {
            get { return _sequence; }
            set { SetValue(ref _sequence, value); }
        }

        private ILocalizationViewModel _localization;
        public ILocalizationViewModel Localization
        {
            get { return _localization; }
            private set { SetValue(ref _localization, value); }
        }

        public Scene(ILocalizationViewModel localizationViewModel)
        {
            _localization = localizationViewModel ?? throw new ArgumentNullException(nameof(localizationViewModel));
        }
    }
}
