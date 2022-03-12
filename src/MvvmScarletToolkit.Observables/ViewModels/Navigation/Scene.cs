using CommunityToolkit.Mvvm.ComponentModel;
using System;

namespace MvvmScarletToolkit.Observables
{
    public partial class Scene : ObservableObject
    {
        [ObservableProperty]
        private object? _content;

        [ObservableProperty]
        private bool _isSelected;

        [ObservableProperty]
        public int _sequence;

        public ILocalizationViewModel Localization { get; }

        public Scene(in ILocalizationViewModel localizationViewModel)
        {
            Localization = localizationViewModel ?? throw new ArgumentNullException(nameof(localizationViewModel));
        }
    }
}
