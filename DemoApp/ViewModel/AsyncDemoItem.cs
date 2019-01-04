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

        public AsyncDemoItem(ICommandManager commandManager)
            : base(commandManager)
        {
            DisplayName = "unknown";
        }

        public AsyncDemoItem(ICommandManager commandManager, string displayName)
            : this(commandManager)
        {
            DisplayName = displayName;
        }

        protected override async Task LoadInternal(CancellationToken token)
        {
            using (BusyStack.GetToken())
            {
                await Task.Delay(2000, token).ConfigureAwait(true);

                IsLoaded = true;
            }
        }

        protected override async Task RefreshInternal(CancellationToken token)
        {
            using (BusyStack.GetToken())
            {
                await Task.Delay(2000).ConfigureAwait(true);
            }
        }

        protected override Task UnloadInternalAsync()
        {
            IsLoaded = false;
            return Task.CompletedTask;
        }
    }
}
