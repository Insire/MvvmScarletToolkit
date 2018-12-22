using MvvmScarletToolkit.Abstractions;
using MvvmScarletToolkit.Commands;
using MvvmScarletToolkit.Observables;
using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;

namespace MvvmScarletToolkit.FileSystemBrowser
{
    public class FileSystemOptionsViewModel : ViewModelBase
    {
        private readonly IScarletDispatcher _dispatcher;

        private bool _displayListView;
        [Bindable(true, BindingDirection.TwoWay)]
        public bool DisplayListView
        {
            get { return _displayListView; }
            set { SetValue(ref _displayListView, value); }
        }

        [Bindable(true, BindingDirection.OneWay)]
        public IExtendedAsyncCommand DisplayDetailsAsListCommand { get; }

        [Bindable(true, BindingDirection.OneWay)]
        public IExtendedAsyncCommand DisplayDetailsAsIconsCommand { get; }

        public FileSystemOptionsViewModel(IScarletDispatcher dispatcher)
        {
            _dispatcher = dispatcher ?? throw new ArgumentNullException(nameof(dispatcher));

            DisplayDetailsAsListCommand = AsyncCommand.Create(() => ToggleAsListViewInternal(CancellationToken.None), () => !IsBusy && !DisplayListView);
            DisplayDetailsAsIconsCommand = AsyncCommand.Create(() => ToggleAsIconsInternal(CancellationToken.None), () => !IsBusy && DisplayListView);
        }

        private async Task ToggleAsListViewInternal(CancellationToken token)
        {
            using (BusyStack.GetToken())
            {
                await _dispatcher.Invoke(() => DisplayListView = true).ConfigureAwait(false);
            }
        }

        private async Task ToggleAsIconsInternal(CancellationToken token)
        {
            using (BusyStack.GetToken())
            {
                await _dispatcher.Invoke(() => DisplayListView = false).ConfigureAwait(false);
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
