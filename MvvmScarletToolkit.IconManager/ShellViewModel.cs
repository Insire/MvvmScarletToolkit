using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Data;

namespace MvvmScarletToolkit.IconManager
{
    public sealed class ShellViewModel : ObservableObject
    {
        private readonly BusyStack _busyStack;
        private readonly PublicApis _publicApis;
        private readonly ObservableCollection<string> _categories;

        private bool _isBusy;
        public bool IsBusy
        {
            get { return _isBusy; }
            private set { SetValue(ref _isBusy, value); }
        }

        private string _selectedCategory;
        public string SelectedCategory
        {
            get { return _selectedCategory; }
            set { SetValue(ref _selectedCategory, value); }
        }

        public IAsyncCommand LoadCommand { get; }
        public ICollectionView CategoryView { get; }

        public ShellViewModel()
        {
            _categories = new ObservableCollection<string>();
            _busyStack = new BusyStack((hasItems) => IsBusy = hasItems);
            _publicApis = new PublicApis();

            CategoryView = CollectionViewSource.GetDefaultView(_categories);

            LoadCommand = AsyncCommand.Create(Load, CanLoad);
        }

        private async Task Load(CancellationToken token)
        {
            using (_busyStack.GetToken())
            {
                var categories = await _publicApis.GetCategories(token).ConfigureAwait(false);
                
            }
        }

        private bool CanLoad()
        {
            return !IsBusy;
        }
    }
}
