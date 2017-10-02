using MvvmScarletToolkit;
using System.Collections.ObjectModel;

namespace DemoApp
{
    public class DataContextSchenanigansViewModel : ObservableObject
    {
        private DemoItem _selectedItem;
        public DemoItem SelectedItem
        {
            get { return _selectedItem; }
            set { SetValue(ref _selectedItem, value); }
        }

        private ObservableCollection<DemoItem> _items;
        public ObservableCollection<DemoItem> Items
        {
            get { return _items; }
            set { SetValue(ref _items, value); }
        }

        public DataContextSchenanigansViewModel()
        {
            Items = new ObservableCollection<DemoItem>(new[]
            {
                new DemoItem
                {
                    DisplayName = "Test 1",
                },
                new DemoItem
                {
                    DisplayName = "Test 2",
                },
                new DemoItem
                {
                    DisplayName = "Test 3",
                },
            });

            SelectedItem = new DemoItem
            {
                DisplayName = "Test 4",
            };
        }
    }
}
