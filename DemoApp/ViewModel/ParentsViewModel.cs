using MvvmScarletToolkit;
using System.Collections.ObjectModel;

namespace DemoApp
{
    public class ParentsViewModel : ObservableObject
    {
        private ObservableCollection<ParentViewModel> _items;
        public ObservableCollection<ParentViewModel> Items
        {
            get { return _items; }
            set { SetValue(ref _items, value); }
        }

        private ParentViewModel _selectedItem;
        public ParentViewModel SelectedItem
        {
            get { return _selectedItem; }
            set { SetValue(ref _selectedItem, value); }
        }

        public ParentsViewModel()
        {
            Items = new ObservableCollection<ParentViewModel>
            {
                new ParentViewModel(),
                new ParentViewModel(),
                new ParentViewModel(),
                new ParentViewModel(),
            };

            SelectedItem = Items[0];
        }
    }
}
