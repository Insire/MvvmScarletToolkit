using MvvmScarletToolkit.Abstractions;
using MvvmScarletToolkit.Observables;
using System.Threading;
using System.Threading.Tasks;

namespace DemoApp
{
    public class AsyncDemoItem : ViewModelBase
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

        protected override async Task Load(CancellationToken token)
        {
            await Task.Delay(2000, token).ConfigureAwait(true);
        }

        protected override async Task Refresh(CancellationToken token)
        {
            await Task.Delay(2000).ConfigureAwait(true);
        }

        protected override Task Unload(CancellationToken token)
        {
            IsLoaded = false;
            return Task.CompletedTask;
        }
    }
}
