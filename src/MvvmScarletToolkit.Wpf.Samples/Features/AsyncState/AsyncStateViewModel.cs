using MvvmScarletToolkit.Observables;
using System.Threading;
using System.Threading.Tasks;

namespace MvvmScarletToolkit.Wpf.Samples
{
    public sealed class AsyncStateViewModel : BusinessViewModelBase
    {
        private string _displayName;
        public string DisplayName
        {
            get { return _displayName; }
            set { SetProperty(ref _displayName, value); }
        }

        private bool _isSelected;
        public bool IsSelected
        {
            get { return _isSelected; }
            set { SetProperty(ref _isSelected, value); }
        }

        public AsyncStateViewModel(IScarletCommandBuilder commandBuilder)
            : base(commandBuilder)
        {
            DisplayName = "unknown";
        }

        public AsyncStateViewModel(IScarletCommandBuilder commandBuilder, string displayName)
            : this(commandBuilder)
        {
            DisplayName = displayName;
        }

        protected override Task UnloadInternal(CancellationToken token)
        {
            return Task.Delay(2000, token);
        }

        protected override Task RefreshInternal(CancellationToken token)
        {
            return Task.Delay(2000, token);
        }
    }
}
