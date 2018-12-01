using MvvmScarletToolkit.Abstractions;
using MvvmScarletToolkit.Commands;
using MvvmScarletToolkit.Observables;
using System.Threading.Tasks;
using System.Windows.Input;

namespace DemoApp
{
    public class LogItems : ViewModelListBase<LogItem>
    {
        public ICommand AddCommand { get; }

        public LogItems(IScarletDispatcher dispatcher)
            : base(dispatcher)
        {
            AddCommand = AsyncCommand.Create(AddNew, CanAddNew);
        }

        public async Task AddNew()
        {
            var item = new LogItem();
            await Add(item).ConfigureAwait(false);

            SelectedItem = item;
        }

        public bool CanAddNew()
        {
            return Items != null;
        }
    }
}
