using MvvmScarletToolkit.Observables;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace MvvmScarletToolkit.Wpf.Samples
{
    public sealed class DataEntryViewModel : BusinessViewModelBase
    {
        private string _message;
        public string Message
        {
            get { return _message; }
            set { SetProperty(ref _message, value); }
        }

        private DateTime _createdOn;
        public DateTime CreatedOn
        {
            get { return _createdOn; }
            set { SetProperty(ref _createdOn, value); }
        }

        private bool _isSelected;
        public bool IsSelected
        {
            get { return _isSelected; }
            set { SetProperty(ref _isSelected, value); }
        }

        public DataEntryViewModel(IScarletCommandBuilder commandBuilder)
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
