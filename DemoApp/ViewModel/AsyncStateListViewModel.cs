using MvvmScarletToolkit;
using MvvmScarletToolkit.Observables;
using System.Collections.ObjectModel;

namespace DemoApp
{
    public class AsyncStateListViewModel : ObservableObject
    {
        private AsyncStateViewModel _selectedItem;
        public AsyncStateViewModel SelectedItem
        {
            get { return _selectedItem; }
            set { SetValue(ref _selectedItem, value); }
        }

        private ObservableCollection<AsyncStateViewModel> _items;
        public ObservableCollection<AsyncStateViewModel> Items
        {
            get { return _items; }
            set { SetValue(ref _items, value); }
        }

        private ObservableCollection<AsyncStateViewModel> _selectedItems;
        public ObservableCollection<AsyncStateViewModel> SelectedItems
        {
            get { return _selectedItems; }
            set { SetValue(ref _selectedItems, value); }
        }

        private string _filterText;
        public string FilterText
        {
            get { return _filterText; }
            set { SetValue(ref _filterText, value); }
        }

        public AsyncStateListViewModel(ICommandBuilder commandBuilder)
        {
            Items = new ObservableCollection<AsyncStateViewModel>();
            _selectedItems = new ObservableCollection<AsyncStateViewModel>();

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