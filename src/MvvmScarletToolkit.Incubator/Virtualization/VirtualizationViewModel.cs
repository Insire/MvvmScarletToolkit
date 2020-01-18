using MvvmScarletToolkit.Abstractions;
using MvvmScarletToolkit.Observables;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MvvmScarletToolkit
{
    internal class VirtualizationViewModel<TViewModel> : ObservableObject
        where TViewModel : class, IVirtualizationViewModel
    {
        private readonly Func<TViewModel> _factory;
        private readonly IScarletDispatcher _dispatcher;

        private VirtualizationViewModelState _state;
        public VirtualizationViewModelState State
        {
            get { return _state; }
            private set { SetValue(ref _state, value); }
        }

        private TViewModel _viewModel;
        public TViewModel ViewModel
        {
            get { return _viewModel; }
            private set { SetValue(ref _viewModel, value); }
        }

        private ICommand _deflateCommand;
        public ICommand DeflateCommand
        {
            get { return _deflateCommand; }
            private set { SetValue(ref _deflateCommand, value); }
        }

        private IConcurrentCommand _expandCommand;
        public IConcurrentCommand ExpandCommand
        {
            get { return _expandCommand; }
            private set { SetValue(ref _expandCommand, value); }
        }

        public VirtualizationViewModel(ICommandBuilder commandBuilder, IScarletCommandManager commandManager, IScarletDispatcher dispatcher, Func<TViewModel> factory)
        {
            _factory = factory ?? throw new ArgumentNullException(nameof(factory));
            _dispatcher = dispatcher ?? throw new ArgumentNullException(nameof(dispatcher));

            DeflateCommand = commandBuilder
                .Create(Deflate, CanDeflate)
                .WithSingleExecution(commandManager)
                .WithCancellation()
                .Build();

            ExpandCommand = commandBuilder
                .Create(Expand, CanExpand)
                .WithSingleExecution(commandManager)
                .WithCancellation()
                .Build();
        }

        public async Task Expand(CancellationToken token)
        {
            if (!(ViewModel is null))
            {
                return;
            }

            ViewModel = _factory();

            if (!ViewModel.CanLoad())
            {
                return;
            }

            await _dispatcher.Invoke(() => State = VirtualizationViewModelState.Expanding);
            await ViewModel.Load(token).ConfigureAwait(false);
            await _dispatcher.Invoke(() => State = VirtualizationViewModelState.Expanded);
        }

        public bool CanExpand()
        {
            return ViewModel is null;
        }

        public async Task Deflate(CancellationToken token)
        {
            if (ViewModel is null)
            {
                return;
            }

            if (!ViewModel.CanUnload())
            {
                return;
            }

            await _dispatcher.Invoke(() => State = VirtualizationViewModelState.Deflating);
            await ViewModel.Unload(token).ConfigureAwait(false);
            await _dispatcher.Invoke(() => State = VirtualizationViewModelState.Deflated);

            ViewModel = null; // enables GC to collect the viewmodel instance, if there are no other strong references to this instance
        }

        public bool CanDeflate()
        {
            return ViewModel != null;
        }
    }
}
