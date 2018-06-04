using MvvmScarletToolkit;
using System.Collections.ObjectModel;

namespace DemoApp
{
    public class DataContextSchenanigansViewModel : ObservableObject
    {
        private AsyncDemoItem _selectedItem;
        public AsyncDemoItem SelectedItem
        {
            get { return _selectedItem; }
            set { SetValue(ref _selectedItem, value); }
        }

        private ObservableCollection<AsyncDemoItem> _items;
        public ObservableCollection<AsyncDemoItem> Items
        {
            get { return _items; }
            set { SetValue(ref _items, value); }
        }

        public DataContextSchenanigansViewModel()
        {
            Items = new ObservableCollection<AsyncDemoItem>();
            for (var i = 0; i < 10; i++)
            {

                Items.Add(new AsyncDemoItem
                {
                    DisplayName = "Test X",
                });
            }

            SelectedItem = new AsyncDemoItem
            {
                DisplayName = "Test X",
            };
        }
    }
}
