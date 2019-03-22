using MvvmScarletToolkit.Commands;
using MvvmScarletToolkit.Observables;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;

namespace DemoApp
{
    public class LogItems : ViewModelListBase<LogItem>
    {
        public ICommand AddCommand { get; }

        public LogItems(CommandBuilder commandBuilder)
            : base(commandBuilder)
        {
            AddCommand = CommandBuilder.Create(AddNew, CanAddNew).Build();
        }

        public async Task AddNew()
        {
            var item = new LogItem();
            await Add(item).ConfigureAwait(false);
        }

        public bool CanAddNew()
        {
            return Items != null;
        }

        protected override Task Load(CancellationToken token)
        {
            IsLoaded = true;
            return Task.CompletedTask;
        }

        protected override async Task Unload(CancellationToken token)
        {
            await Clear(token).ConfigureAwait(false);
            IsLoaded = false;
        }

        protected override Task Refresh(CancellationToken token)
        {
            return Task.CompletedTask;
        }
    }
}
