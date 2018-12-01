using MvvmScarletToolkit.Abstractions;
using MvvmScarletToolkit.Commands;
using MvvmScarletToolkit.Observables;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MvvmScarletToolkit
{
    internal class VirtualizationViewModel<TViewModel> : ObservableObject
        where TViewModel : class
    {
        private bool _isExpanded;
        public bool IsExpanded
        {
            get { return _isExpanded; }
            private set { SetValue(ref _isExpanded, value); }
        }

        private VirtualizationViewModelState _state;
        public VirtualizationViewModelState State // TODO add implementation details
        {
            get { return _state; }
            private set { SetValue(ref _state, value); }
        }

        private TViewModel _viewModel;
        public TViewModel ViewModel
        {
            get { return _viewModel; }
            set { SetValue(ref _viewModel, value); }
        }

        private ICommand _deflateCommand;
        public ICommand DeflateCommand
        {
            get { return _deflateCommand; }
            private set { SetValue(ref _deflateCommand, value); }
        }

        private IAsyncCommand _expandCommand;
        public IAsyncCommand ExpandCommand
        {
            get { return _expandCommand; }
            private set { SetValue(ref _expandCommand, value); }
        }

        public VirtualizationViewModel()
        {
            DeflateCommand = new RelayCommand(Deflate, CanDeflate);
            ExpandCommand = AsyncCommand.Create(Expand, CanExpand);
        }

        public async Task Expand()
        {
            if (ViewModel != null)
                return;

            //ViewModel = await _dataProvider.Get(Id).ConfigureAwait(true);
            IsExpanded = true;
        }

        public bool CanExpand()
        {
            return ViewModel == null && !IsExpanded;
        }

        public void Deflate()
        {
            ViewModel = null; // enables GC
            IsExpanded = false;
        }

        public bool CanDeflate()
        {
            return ViewModel != null && IsExpanded;
        }
    }
}
