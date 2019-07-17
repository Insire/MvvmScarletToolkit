using MvvmScarletToolkit;
using MvvmScarletToolkit.Observables;
using System.Threading;
using System.Threading.Tasks;

namespace DemoApp
{
    public class AsyncDemoItem : BusinessViewModelBase
    {
        private string _displayName;
        public string DisplayName
        {
            get { return _displayName; }
            set { SetValue(ref _displayName, value); }
        }

        private bool _isSelected;
        public bool IsSelected
        {
            get { return _isSelected; }
            set { SetValue(ref _isSelected, value); }
        }

        public AsyncDemoItem(ICommandBuilder commandBuilder)
            : base(commandBuilder)
        {
            DisplayName = "unknown";
        }

        public AsyncDemoItem(ICommandBuilder commandBuilder, string displayName)
            : this(commandBuilder)
        {
            DisplayName = displayName;
        }

        protected override Task UnloadInternal(CancellationToken token)
        {
            return Task.Delay(2000);
        }

        protected override Task RefreshInternal(CancellationToken token)
        {
            return Task.Delay(2000);
        }
    }
}
