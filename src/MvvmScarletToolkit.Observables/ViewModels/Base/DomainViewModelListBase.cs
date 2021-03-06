using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace MvvmScarletToolkit.Observables
{
    /// <summary>
    /// Collection ViewModelBase that adds support for paged loading
    /// </summary>
    /// <typeparam name="TViewModel"></typeparam>
    public abstract class DomainViewModelListBase<TViewModel> : BusinessViewModelListBase<TViewModel>
        where TViewModel : class, INotifyPropertyChanged
    {
        private int _total;
        [Bindable(true, BindingDirection.TwoWay)]
        public int Total
        {
            get { return _total; }
            protected set { SetProperty(ref _total, value); }
        }

        private int _pageSize;
        [Bindable(true, BindingDirection.TwoWay)]
        public int PageSize
        {
            get { return _pageSize; }
            set { SetProperty(ref _pageSize, value); }
        }

        private int _pageIndex;
        [Bindable(true, BindingDirection.TwoWay)]
        public int PageIndex
        {
            get { return _pageIndex; }
            set { SetProperty(ref _pageIndex, value); }
        }

        [Bindable(true, BindingDirection.OneWay)]
        public PagingViewModel<TViewModel> Paging { get; }

        protected DomainViewModelListBase(in IScarletCommandBuilder commandBuilder, in IEnumerable<int> pageSizes)
            : base(commandBuilder)
        {
            Paging = new PagingViewModel<TViewModel>(commandBuilder, this, new ReadOnlyObservableCollection<int>(new ObservableCollection<int>(pageSizes)));
        }

        protected override void Dispose(bool disposing)
        {
            if (IsDisposed)
            {
                return;
            }

            if (disposing)
            {
                Paging.Dispose();
            }

            base.Dispose(disposing);
        }
    }
}
