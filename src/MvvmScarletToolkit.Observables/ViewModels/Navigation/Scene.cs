using CommunityToolkit.Mvvm.ComponentModel;
using System;

namespace MvvmScarletToolkit.Observables
{
    public partial class Scene : ObservableObject
    {
        [ObservableProperty]
        public partial object? Content { get; set; }

        [ObservableProperty]
        public partial bool IsSelected { get; set; }

        [ObservableProperty]
        public partial int Sequence { get; set; }
        public ILocalizationViewModel Localization { get; }

        public Scene(in ILocalizationViewModel localizationViewModel)
        {
            Localization = localizationViewModel ?? throw new ArgumentNullException(nameof(localizationViewModel));
        }
    }
}
