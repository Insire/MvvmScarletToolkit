using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MvvmScarletToolkit.Observables
{
    /// <summary>
    /// ViewModel that adds paging support to <see cref="DomainViewModelListBase{TViewModel}"/>
    /// </summary>
    /// <typeparam name="TViewModel"></typeparam>
    public sealed class PagingViewModel<TViewModel> : ViewModelBase
        where TViewModel : class, INotifyPropertyChanged
    {
        private readonly DomainViewModelListBase<TViewModel> _viewModel;

        private bool _disposed;

        [Bindable(true, BindingDirection.OneWay)]
        public ICommand NextCommand { get; }

        [Bindable(true, BindingDirection.OneWay)]
        public ICommand PreviousCommand { get; }

        [Bindable(true, BindingDirection.OneWay)]
        public ICommand FirstCommand { get; }

        [Bindable(true, BindingDirection.OneWay)]
        public ICommand LastCommand { get; }

        [Bindable(true, BindingDirection.OneWay)]
        public ReadOnlyObservableCollection<int> PageSizes { get; }

        public PagingViewModel(in IScarletCommandBuilder commandBuilder, in DomainViewModelListBase<TViewModel> viewModel, in ReadOnlyObservableCollection<int> pageSizes)
            : base(commandBuilder)
        {
            _viewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
            PageSizes = pageSizes ?? throw new ArgumentNullException(nameof(viewModel));

            NextCommand = commandBuilder.Create(Next, CanNext)
                .WithSingleExecution()
                .WithBusyNotification(BusyStack)
                .WithCancellation()
                .Build();

            PreviousCommand = commandBuilder.Create(Previous, CanPrevious)
                .WithSingleExecution()
                .WithBusyNotification(BusyStack)
                .WithCancellation()
                .Build();

            FirstCommand = commandBuilder.Create(First, CanFirst)
                .WithSingleExecution()
                .WithBusyNotification(BusyStack)
                .WithCancellation()
                .Build();

            LastCommand = commandBuilder.Create(Last, CanLast)
                .WithSingleExecution()
                .WithBusyNotification(BusyStack)
                .WithCancellation()
                .Build();
        }

        private int GetNext()
        {
            return (_viewModel.PageIndex + _viewModel.PageSize < _viewModel.Total - _viewModel.PageSize - 1)
            ? _viewModel.PageIndex + _viewModel.PageSize
            : _viewModel.Total;
        }

        private Task Next(CancellationToken token)
        {
            return Set(GetNext(), token);
        }

        private bool CanNext()
        {
            return _viewModel.CanRefresh() && GetNext() < _viewModel.Total;
        }

        private int GetPrevious()
        {
            return (_viewModel.PageIndex - 1 - _viewModel.PageSize > 0)
            ? _viewModel.PageIndex - 1
            : 0;
        }

        private Task Previous(CancellationToken token)
        {
            return Set(GetPrevious(), token);
        }

        private bool CanPrevious()
        {
            return _viewModel.CanRefresh() && GetPrevious() > 0;
        }

        private int GetFirst()
        {
            return 0;
        }

        private Task First(CancellationToken token)
        {
            return Set(GetFirst(), token);
        }

        private bool CanFirst()
        {
            return _viewModel.CanRefresh() && _viewModel.PageIndex > GetFirst();
        }

        private int GetLast()
        {
            return _viewModel.Total - _viewModel.PageSize - 1;
        }

        private Task Last(CancellationToken token)
        {
            return Set(GetLast(), token);
        }

        private bool CanLast()
        {
            return _viewModel.CanRefresh() && _viewModel.PageIndex < _viewModel.Total;
        }

        private async Task Set(int index, CancellationToken token)
        {
            await Dispatcher.Invoke(() => _viewModel.PageIndex = index, token).ConfigureAwait(false);
            await _viewModel.Refresh(token).ConfigureAwait(false);
        }

        protected override void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            base.Dispose(disposing);
            _disposed = true;
        }
    }
}
