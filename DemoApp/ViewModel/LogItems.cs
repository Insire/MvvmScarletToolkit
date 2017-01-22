using System.Windows.Input;
using MvvmScarletToolkit;

namespace DemoApp
{
    public class LogItems : ViewModelListBase<LogItem>
    {
        public ICommand AddCommand { get; private set; }

        public LogItems()
        {
            AddCommand = new RelayCommand(AddNew, CanAddNew);
        }

        public void AddNew()
        {
            var item = new LogItem();
            Items.Add(item);

            SelectedItem = item;
        }

        public bool CanAddNew()
        {
            return Items != null;
        }
    }
}
