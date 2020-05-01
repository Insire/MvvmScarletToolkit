using MvvmScarletToolkit.Observables;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace MvvmScarletToolkit.Samples
{
    public class LogItem : BusinessViewModelBase, IVirtualizationViewModel
    {
        private string _message;
        public string Message
        {
            get { return _message; }
            set { SetValue(ref _message, value); }
        }

        private DateTime _createdOn;
        public DateTime CreatedOn
        {
            get { return _createdOn; }
            set { SetValue(ref _createdOn, value); }
        }

        private bool _isSelected;
        public bool IsSelected
        {
            get { return _isSelected; }
            set { SetValue(ref _isSelected, value); }
        }

        public LogItem(IScarletCommandBuilder commandBuilder)
            : base(commandBuilder)
        {
        }

        protected override Task UnloadInternal(CancellationToken token)
        {
            CreatedOn = DateTime.MinValue;
            Message = "";

            return Task.CompletedTask;
        }

        protected override Task RefreshInternal(CancellationToken token)
        {
            CreatedOn = DateTime.UtcNow;
            Message = CreatedOn.GetHashCode().ToString();

            return Task.CompletedTask;
        }
    }
}
