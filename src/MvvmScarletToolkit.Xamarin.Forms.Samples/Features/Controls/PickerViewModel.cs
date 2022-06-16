using CommunityToolkit.Mvvm.ComponentModel;
using MvvmScarletToolkit.Observables;
using System.Collections.ObjectModel;

namespace MvvmScarletToolkit.Xamarin.Forms.Samples.Features.Controls
{
    internal sealed partial class PickerViewModel : ViewModelBase
    {
        [ObservableProperty]
        private string _selectedItem;

        public ObservableCollection<string> Items { get; }

        public PickerViewModel()
               : base(ScarletCommandBuilder.Default)
        {
            Items = new ObservableCollection<string>()
            {
                "Test 0",
                "Test 1",
                "Test 2",
                "Test 3",
                "Test 4",
            };

            SelectedItem = Items[0];
        }
    }
}
