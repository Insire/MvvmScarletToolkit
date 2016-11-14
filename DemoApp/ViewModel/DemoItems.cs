using System.Windows.Input;
using MvvmScarletToolkit;

namespace DemoApp
{
    public class DemoItems : ViewModelBase<DemoItem>
    {
        public ICommand AddCommand { get; private set; }

        public DemoItems()
        {
            AddCommand = new RelayCommand(AddNew, CanAddNew);
        }

        public void AddNew()
        {
            var item = new DemoItem();
            Items.Add(item);

            SelectedItem = item;
        }

        public bool CanAddNew()
        {
            return Items != null;
        }
    }
}
