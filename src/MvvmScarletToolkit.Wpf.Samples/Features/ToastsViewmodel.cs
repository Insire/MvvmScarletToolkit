using CommunityToolkit.Mvvm.ComponentModel;
using MvvmScarletToolkit.Observables;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MvvmScarletToolkit.Wpf.Samples.Features
{
    public sealed partial class ToastsViewModel : ViewModelBase
    {
        public ICommand ShowToastCommand { get; }
        public ICommand ShowManyToastsCommand { get; }

        [ObservableProperty]
        private ToastType _toastType;

        [ObservableProperty]
        private string _title;

        [ObservableProperty]
        private string _body;

        [ObservableProperty]
        private bool _persist;

        public ToastsViewModel(IScarletCommandBuilder commandBuilder)
            : base(commandBuilder)
        {
            ShowToastCommand = commandBuilder
                .Create(ShowToastImpl)
                .Build();

            ShowManyToastsCommand = commandBuilder
                .Create(ShowManyToastsImpl)
                .Build();

            _title = "Toast-Title";
            _body = "Toast-Body";
            _toastType = ToastType.Success;
        }

        private Task ShowToastImpl(CancellationToken token)
        {
            ToastService.Default.Show(Title, Body, ToastType, Persist);

            return Task.CompletedTask;
        }

        private Task ShowManyToastsImpl(CancellationToken token)
        {
            for (var i = 0; i < 100; i++)
            {
                ToastService.Default.Show(Title, Body, ToastType, Persist);
            }

            return Task.CompletedTask;
        }
    }
}
