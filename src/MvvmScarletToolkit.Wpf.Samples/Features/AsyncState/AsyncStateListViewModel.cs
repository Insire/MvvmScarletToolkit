using Microsoft.Toolkit.Mvvm.ComponentModel;
using System.Collections;
using System.Collections.ObjectModel;

namespace MvvmScarletToolkit.Wpf.Samples
{
    public class AsyncStateListViewModel : ObservableObject
    {
        private AsyncStateViewModel _selectedItem;
        public AsyncStateViewModel SelectedItem
        {
            get { return _selectedItem; }
            set { SetProperty(ref _selectedItem, value); }
        }

        private ObservableCollection<AsyncStateViewModel> _items;
        public ObservableCollection<AsyncStateViewModel> Items
        {
            get { return _items; }
            set { SetProperty(ref _items, value); }
        }

        public IList SelectedItems { get; }

        private string _filterText;
        public string FilterText
        {
            get { return _filterText; }
            set { SetProperty(ref _filterText, value); }
        }

        public AsyncStateListViewModel(IScarletCommandBuilder commandBuilder)
        {
            Items = new ObservableCollection<AsyncStateViewModel>();
            SelectedItems = new ObservableCollection<AsyncStateViewModel>();

            for (var i = 0; i < 10; i++)
            {
                Items.Add(new AsyncStateViewModel(commandBuilder)
                {
                    DisplayName = "Test " + i,
                });
            }

            SelectedItem = new AsyncStateViewModel(commandBuilder)
            {
                DisplayName = "Test X",
            };
        }
    }
}
