using MvvmScarletToolkit.Commands;
using MvvmScarletToolkit.Observables;
using System.Windows.Input;

namespace DemoApp
{
    public class LogItems : ViewModelListBase<LogItem>
    {
        public ICommand AddCommand { get; }

        public LogItems()
        {
            AddCommand = new RelayCommand(AddNew, CanAddNew);
        }

        public void AddNew()
        {
            var item = new LogItem();
            Add(item);

            SelectedItem = item;
        }

        public bool CanAddNew()
        {
            return Items != null;
        }
    }
}
