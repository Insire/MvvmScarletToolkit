using MvvmScarletToolkit;
using MvvmScarletToolkit.Commands;
using System.Windows.Input;

namespace DemoApp
{
    public class DemoItems : ViewModelListBase<DemoItem>
    {
        public ICommand AddCommand { get; }

        public DemoItems()
        {
            AddCommand = new RelayCommand(AddNew, CanAddNew);
        }

        public void AddNew()
        {
            var item = new DemoItem();
            Add(item);

            SelectedItem = item;
        }

        public bool CanAddNew()
        {
            return Items != null;
        }
    }
}
