using MvvmScarletToolkit.Abstractions;
using MvvmScarletToolkit.Observables;
using System.Threading;
using System.Threading.Tasks;

namespace DemoApp
{
    public class DataContextSchenanigansViewModel : ViewModelListBase<AsyncDemoItem>
    {
        public DataContextSchenanigansViewModel(ICommandBuilder commandBuilder)
            : base(commandBuilder)
        {
        }

        protected override async Task Load(CancellationToken token)
        {
            for (var i = 0; i < 10; i++)
            {
                var item = new AsyncDemoItem(CommandBuilder)
                {
                    DisplayName = "Test " + i,
                };

                await Add(item).ConfigureAwait(false);
            }
        }

        protected override Task Refresh(CancellationToken token)
        {
            return Task.CompletedTask;
        }
    }
}
