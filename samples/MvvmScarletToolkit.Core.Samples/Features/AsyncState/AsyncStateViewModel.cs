using CommunityToolkit.Mvvm.ComponentModel;
using MvvmScarletToolkit.Observables;

namespace MvvmScarletToolkit.Core.Samples.Features.AsyncState
{
    public sealed partial class AsyncStateViewModel : BusinessViewModelBase
    {
        [ObservableProperty]
        public partial string DisplayName { get; set; }

        [ObservableProperty]
        public partial bool IsSelected { get; set; }

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
