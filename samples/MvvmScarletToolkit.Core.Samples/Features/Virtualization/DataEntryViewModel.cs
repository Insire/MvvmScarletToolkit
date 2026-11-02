using CommunityToolkit.Mvvm.ComponentModel;
using MvvmScarletToolkit.Observables;

namespace MvvmScarletToolkit.Core.Samples.Features.Virtualization
{
    public sealed partial class DataEntryViewModel : BusinessViewModelBase
    {
        [ObservableProperty]
        public partial string Message { get; set; }

        [ObservableProperty]
        public partial DateTime CreatedOn { get; set; }

        [ObservableProperty]
        public partial Guid Id { get; set; }

        [ObservableProperty]
        public partial bool IsSelected { get; set; }

        public DataEntryViewModel(IScarletCommandBuilder commandBuilder)
            : base(commandBuilder)
        {
            Id = Guid.NewGuid();
            Message = string.Empty;
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
