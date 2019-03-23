using MvvmScarletToolkit.Commands;
using MvvmScarletToolkit.Observables;
using System.Diagnostics;
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

        public AsyncDemoItem(CommandBuilder commandBuilder)
            : base(commandBuilder)
        {
            DisplayName = "unknown";
        }

        public AsyncDemoItem(CommandBuilder commandBuilder, string displayName)
            : this(commandBuilder)
        {
            DisplayName = displayName;

            RefreshCommand.PropertyChanged += RefreshCommand_PropertyChanged;
        }

        private void RefreshCommand_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            Debug.WriteLine($"RefreshCommand of {DisplayName} updated (IsBusy: {RefreshCommand.IsBusy}");
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
