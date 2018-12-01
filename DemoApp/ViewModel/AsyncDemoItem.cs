using MvvmScarletToolkit.Abstractions;
using MvvmScarletToolkit.Commands;
using MvvmScarletToolkit.Observables;
using System.Threading.Tasks;

namespace DemoApp
{
    public class AsyncDemoItem : ObservableObject
    {
        private readonly BusyStack _busyStack;

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

        private bool _isLoaded;
        public bool IsLoaded
        {
            get { return _isLoaded; }
            private set { SetValue(ref _isLoaded, value); }
        }

        private bool _isBusy;
        public bool IsBusy
        {
            get { return _isBusy; }
            private set { SetValue(ref _isBusy, value); }
        }

        private IAsyncCommand _loadCommand;
        public IAsyncCommand LoadCommand
        {
            get { return _loadCommand; }
            private set { SetValue(ref _loadCommand, value); }
        }

        private IAsyncCommand _doStuffCommand;
        public IAsyncCommand DoStuffCommand
        {
            get { return _doStuffCommand; }
            private set { SetValue(ref _doStuffCommand, value); }
        }

        public AsyncDemoItem()
        {
            DisplayName = "unknown";
            LoadCommand = AsyncCommand.Create(Load, () => !IsLoaded && !IsBusy);
            DoStuffCommand = AsyncCommand.Create(DoStuff, CanDoStuff);

            _busyStack = new BusyStack((hasItems) => IsBusy = hasItems);
        }

        public AsyncDemoItem(string displayName) : this()
        {
            DisplayName = displayName;
        }

        private async Task Load()
        {
            using (_busyStack.GetToken())
            {
                await Task.Delay(2000).ConfigureAwait(true);

                IsLoaded = true;
            }
        }

        private async Task DoStuff()
        {
            using (_busyStack.GetToken())
            {
                await Task.Delay(2000).ConfigureAwait(true);
            }
        }

        private bool CanDoStuff()
        {
            var result = !IsBusy;

            return result;
        }
    }
}
