using MvvmScarletToolkit;
using MvvmScarletToolkit.Observables;
using System.Threading;
using System.Threading.Tasks;

namespace DemoApp
{
    public class DataContextSchenanigansViewModel : BusinessViewModelListBase<AsyncDemoItem>
    {
        private string _filterText;
        public string FilterText
        {
            get { return _filterText; }
            set { SetValue(ref _filterText, value); }
        }

        public DataContextSchenanigansViewModel(ICommandBuilder commandBuilder)
            : base(commandBuilder)
        {
        }

        protected override async Task RefreshInternal(CancellationToken token)
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
    }
}
