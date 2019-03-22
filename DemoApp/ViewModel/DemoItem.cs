using MvvmScarletToolkit.Commands;
using MvvmScarletToolkit.Observables;
using System.Threading;
using System.Threading.Tasks;

namespace DemoApp
{
    public class DemoItem : ViewModelBase
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

        public DemoItem(CommandBuilder commandBuilder)
            : base(commandBuilder)
        {
            DisplayName = "unknown";
        }

        public DemoItem(CommandBuilder commandBuilder, string displayName)
            : this(commandBuilder)
        {
            DisplayName = displayName;
        }

        protected override Task Load(CancellationToken token)
        {
            IsLoaded = true;
            return Task.CompletedTask;
        }

        protected override Task Unload(CancellationToken token)
        {
            return Task.CompletedTask;
        }

        protected override Task Refresh(CancellationToken token)
        {
            return Task.CompletedTask;
        }
    }
}
