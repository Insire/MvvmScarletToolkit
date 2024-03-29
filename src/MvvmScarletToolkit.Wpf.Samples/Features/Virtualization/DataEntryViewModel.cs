using CommunityToolkit.Mvvm.ComponentModel;
using MvvmScarletToolkit.Observables;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace MvvmScarletToolkit.Wpf.Samples
{
    public sealed partial class DataEntryViewModel : BusinessViewModelBase
    {
        [ObservableProperty]
        private string _message;

        [ObservableProperty]
        private DateTime _createdOn;

        [ObservableProperty]
        private Guid _id;

        [ObservableProperty]
        private bool _isSelected;

        public DataEntryViewModel(IScarletCommandBuilder commandBuilder)
            : base(commandBuilder)
        {
            Id = Guid.NewGuid();
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
