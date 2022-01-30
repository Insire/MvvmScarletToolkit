using MvvmScarletToolkit.Observables;
using System.Collections.ObjectModel;

namespace MvvmScarletToolkit.Xamarin.Forms.Samples.Features.Controls
{
    internal sealed class PickerViewModel : ViewModelBase
    {
        private string _selectedItem;
        public string SelectedItem
        {
            get { return _selectedItem; }
            set { SetProperty(ref _selectedItem, value); }
        }

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
