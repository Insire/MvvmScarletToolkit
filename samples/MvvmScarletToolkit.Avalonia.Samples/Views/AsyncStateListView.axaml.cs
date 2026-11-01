using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using DynamicData;
using DynamicData.Binding;
using MvvmScarletToolkit.Avalonia.Samples.Behaviors;
using MvvmScarletToolkit.Core.Samples.Features.AsyncState;
using System.Collections.ObjectModel;
using System.Reactive.Linq;

namespace MvvmScarletToolkit.Avalonia.Samples.Views
{
    public partial class AsyncStateListView : UserControl
    {
        private static readonly IComparer<AsyncStateViewModel> Sorting = SortExpressionComparer<AsyncStateViewModel>
            .Descending(p => p.IsBusy)
            .ThenByAscending(p => p.DisplayName);

        private IDisposable? _subscription;
        private ReadOnlyObservableCollection<AsyncStateViewModel>? _sorted;
        private AsyncStateViewModel[] _selection = [];
        private AsyncStateViewModel? _selectedItem;
        private readonly ContainerLifecycle<AsyncStateViewModel> _lifecycle;

        public AsyncStateListView()
        {
            InitializeComponent();

            _lifecycle = new ContainerLifecycle<AsyncStateViewModel>(SortedList, OnItemRealized, _ => { });
        }

        private static void OnItemRealized(AsyncStateViewModel item)
        {
            if (item.LoadCommand.CanExecute(null))
            {
                item.LoadCommand.Execute(null);
            }
        }

        protected override void OnDataContextChanged(EventArgs e)
        {
            base.OnDataContextChanged(e);

            _subscription?.Dispose();
            _subscription = null;

            if (DataContext is not AsyncStateListViewModel viewModel)
            {
                return;
            }

            // what a CollectionViewSource with live sorting does in the WPF sample
            var filter = viewModel
                .WhenValueChanged(p => p.FilterText)
                .Select(_ => new Func<AsyncStateViewModel, bool>(viewModel.Filter));

            _subscription = viewModel.Items
                .ToObservableChangeSet()
                .AutoRefresh(p => p.IsBusy)
                .Filter(filter)
                .Sort(Sorting)
                .ObserveOn(SynchronizationContext.Current!)
                .Do(_ => RememberSelection(viewModel))
                .Bind(out _sorted)
                .Do(_ => RestoreSelection(viewModel))
                .Do(_ => ScrollToEnd())
                .Subscribe();

            SortedList.ItemsSource = _sorted;
            SortedTable.ItemsSource = _sorted;
        }

        protected override void OnUnloaded(RoutedEventArgs e)
        {
            base.OnUnloaded(e);

            _subscription?.Dispose();
            _subscription = null;
        }

        // moving an item is a remove and an add for Avalonia, which would deselect everything that gets sorted to another place
        private void RememberSelection(AsyncStateListViewModel viewModel)
        {
            _selection = viewModel.SelectedItems.ToArray();
            _selectedItem = viewModel.SelectedItem;
        }

        private void RestoreSelection(AsyncStateListViewModel viewModel)
        {
            foreach (var item in _selection)
            {
                if (_sorted?.Contains(item) == true && !viewModel.SelectedItems.Contains(item))
                {
                    viewModel.SelectedItems.Add(item);
                }
            }

            if (_selectedItem is not null && viewModel.SelectedItem is null && _sorted?.Contains(_selectedItem) == true)
            {
                viewModel.SelectedItem = _selectedItem;
            }
        }

        private void ScrollToEnd()
        {
            if (Generate.IsChecked == true && _sorted?.Count > 0)
            {
                SortedList.ScrollIntoView(_sorted.Count - 1);
            }
        }

        private void SortedList_SelectionChanged(object? sender, SelectionChangedEventArgs e)
        {
            foreach (var item in e.RemovedItems.OfType<AsyncStateViewModel>())
            {
                item.IsSelected = false;
            }

            foreach (var item in e.AddedItems.OfType<AsyncStateViewModel>())
            {
                item.IsSelected = true;
            }
        }

        private void Generate_IsCheckedChanged(object? sender, RoutedEventArgs e)
        {
            if (DataContext is AsyncStateListViewModel viewModel && sender is ToggleButton toggle)
            {
                (toggle.IsChecked == true ? viewModel.EnableGenerationCommand : viewModel.DisableGenerationCommand).Execute(null);
            }
        }
    }
}
