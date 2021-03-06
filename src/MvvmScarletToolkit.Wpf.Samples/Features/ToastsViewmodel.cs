using MvvmScarletToolkit.Observables;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MvvmScarletToolkit.Wpf.Samples
{
    public sealed class ToastsViewModel : ViewModelBase
    {
        public ICommand ShowToastCommand { get; }
        public ICommand ShowManyToastsCommand { get; }

        private ToastType _toastType;
        public ToastType ToastType
        {
            get { return _toastType; }
            set { SetProperty(ref _toastType, value); }
        }

        private string _title;
        public string Title
        {
            get { return _title; }
            set { SetProperty(ref _title, value); }
        }

        private string _body;
        public string Body
        {
            get { return _body; }
            set { SetProperty(ref _body, value); }
        }

        private bool _persist;
        public bool Persist
        {
            get { return _persist; }
            set { SetProperty(ref _persist, value); }
        }

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
