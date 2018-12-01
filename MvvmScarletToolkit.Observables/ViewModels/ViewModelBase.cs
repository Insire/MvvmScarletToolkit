using MvvmScarletToolkit.Abstractions;
using MvvmScarletToolkit.Commands;
using System.Threading;
using System.Threading.Tasks;

namespace MvvmScarletToolkit.Observables
{
    public abstract class ViewModelBase : ObservableObject
    {
        protected readonly BusyStack BusyStack;

        private bool _isBusy;
        public bool IsBusy
        {
            get { return _isBusy; }
            private set { SetValue(ref _isBusy, value); }
        }

        private bool _isLoaded;
        public bool IsLoaded
        {
            get { return _isLoaded; }
            private set { SetValue(ref _isLoaded, value); }
        }

        public virtual IExtendedAsyncCommand LoadCommand { get; }

        public virtual IExtendedAsyncCommand RefreshCommand { get; }

        public virtual IExtendedAsyncCommand UnloadCommand { get; }

        protected ViewModelBase()
        {
            BusyStack = new BusyStack((hasItems) => IsBusy = hasItems);

            LoadCommand = AsyncCommand.Create(LoadInternal, CanLoad);
            RefreshCommand = AsyncCommand.Create(RefreshInternal, CanRefresh);
            UnloadCommand = AsyncCommand.Create(UnloadInternal, CanUnload);
        }

        protected virtual Task LoadInternal(CancellationToken token)
        {
            using (BusyStack.GetToken())
            {
                IsLoaded = true;
                return Task.CompletedTask;
            }
        }

        protected virtual bool CanLoad()
        {
            return !IsLoaded;
        }

        protected virtual Task UnloadInternal()
        {
            using (BusyStack.GetToken())
            {
                IsLoaded = false;
                return Task.CompletedTask;
            }
        }

        protected virtual bool CanUnload()
        {
            return IsLoaded;
        }

        protected virtual Task RefreshInternal(CancellationToken token)
        {
            using (BusyStack.GetToken())
                return Task.CompletedTask;
        }

        protected virtual bool CanRefresh()
        {
            return IsLoaded;
        }
    }
}
