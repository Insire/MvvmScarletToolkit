using MvvmScarletToolkit.Commands;
using MvvmScarletToolkit.Observables;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;

namespace MvvmScarletToolkit.FileSystemBrowser
{
    public class FileSystemOptionsViewModel : ViewModelBase
    {
        private bool _displayListView;
        [Bindable(true, BindingDirection.TwoWay)]
        public bool DisplayListView
        {
            get { return _displayListView; }
            set { SetValue(ref _displayListView, value); }
        }

        [Bindable(true, BindingDirection.OneWay)]
        public ConcurrentCommandBase DisplayDetailsAsListCommand { get; }

        [Bindable(true, BindingDirection.OneWay)]
        public ConcurrentCommandBase DisplayDetailsAsIconsCommand { get; }

        public FileSystemOptionsViewModel(CommandBuilder commandBuilder)
            : base(commandBuilder)
        {
            DisplayDetailsAsListCommand = commandBuilder.Create(() => ToggleAsListViewInternal(CancellationToken.None), () => !IsBusy && !DisplayListView);
            DisplayDetailsAsIconsCommand = commandBuilder.Create(() => ToggleAsIconsInternal(CancellationToken.None), () => !IsBusy && DisplayListView);
        }

        private async Task ToggleAsListViewInternal(CancellationToken token)
        {
            using (BusyStack.GetToken())
            {
                await Dispatcher.Invoke(() => DisplayListView = true).ConfigureAwait(false);
            }
        }

        private async Task ToggleAsIconsInternal(CancellationToken token)
        {
            using (BusyStack.GetToken())
            {
                await Dispatcher.Invoke(() => DisplayListView = false).ConfigureAwait(false);
            }
        }

        protected override Task LoadInternal(CancellationToken token)
        {
            throw new System.NotImplementedException();
        }

        protected override Task RefreshInternal(CancellationToken token)
        {
            throw new System.NotImplementedException();
        }

        protected override Task UnloadInternalAsync()
        {
            throw new System.NotImplementedException();
        }
    }
}
