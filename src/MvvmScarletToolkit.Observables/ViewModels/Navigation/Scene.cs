using CommunityToolkit.Mvvm.ComponentModel;
using System;

namespace MvvmScarletToolkit.Observables
{
    public class Scene : ObservableObject
    {
        private object? _content;
        public object? Content
        {
            get { return _content; }
            set { SetProperty(ref _content, value); }
        }

        private bool _isSelected;
        public bool IsSelected
        {
            get { return _isSelected; }
            set { SetProperty(ref _isSelected, value); }
        }

        public int _sequence;
        public int Sequence
        {
            get { return _sequence; }
            set { SetProperty(ref _sequence, value); }
        }

        public ILocalizationViewModel Localization { get; }

        public Scene(in ILocalizationViewModel localizationViewModel)
        {
            Localization = localizationViewModel ?? throw new ArgumentNullException(nameof(localizationViewModel));
        }
    }
}
