using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace MvvmScarletToolkit.Observables
{
    /// <summary>
    /// Collection ViewModelBase that adds support for paged loading
    /// </summary>
    /// <typeparam name="TViewModel"></typeparam>
    public abstract partial class DomainViewModelListBase<TViewModel> : BusinessViewModelListBase<TViewModel>
        where TViewModel : class, INotifyPropertyChanged
    {
        [Bindable(true, BindingDirection.TwoWay)]
        public int Total
        {
            get { return field; }
            protected set { SetProperty(ref field, value); }
        }

        [ObservableProperty]
        public partial int PageSize { get; set; }

        [ObservableProperty]
        public partial int PageIndex { get; set; }

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
