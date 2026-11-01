using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DynamicData;
using DynamicData.Binding;
using MvvmScarletToolkit.Wpf.Features.FileSystemBrowser.Directories;
using MvvmScarletToolkit.Wpf.Features.FileSystemBrowser.Drives;
using MvvmScarletToolkit.Wpf.Features.FileSystemBrowser.Files;
using MvvmScarletToolkit.Wpf.Features.FileSystemBrowser.Interfaces;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Linq;

namespace MvvmScarletToolkit.Wpf.Features.FileSystemBrowser
{
    public sealed partial class FileSystemViewModel : ObservableObject, IDisposable
    {
        /// <summary>
        /// Every result is a viewmodel, searching a whole drive must not exhaust the memory.
        /// </summary>
        public const int MaxSearchResults = 10_000;

        private static readonly FileAttributes[] _displayedAttributes =
        [
            FileAttributes.ReadOnly,
            FileAttributes.Hidden,
            FileAttributes.System,
            FileAttributes.Archive,
            FileAttributes.Compressed,
            FileAttributes.Encrypted,
        ];

        private readonly ObservableCollectionExtended<IFileSystemDrive> _items;
        private readonly SourceCache<IFileSystemDrive, string> _cache;
        private readonly ObservableCollectionExtended<IFileSystemChild> _searchResults;
        private readonly ReadOnlyObservableCollection<IFileSystemChild> _readOnlySearchResults;
        private readonly FileSystemViewModelMapper _mapper;
        private readonly CompositeDisposable _compositeDisposable;
        private readonly IScheduler _scheduler;
        private readonly IFileSystemViewModelFactory _factory;

        private CancellationTokenSource? _searchCancellation;
        private CancellationTokenSource? _permissionsCancellation;
        private string? _selectedEntryPath;

        [ObservableProperty, Bindable(true, BindingDirection.OneWay)] public partial bool IsLoaded { get; private set; }
        [ObservableProperty, Bindable(true, BindingDirection.OneWay)] public partial IFileSystemParent? SelectedContainer { get; private set; }
        [ObservableProperty, Bindable(true, BindingDirection.OneWay)] public partial IFileSystemChild? SelectedChild { get; private set; }
        [ObservableProperty, Bindable(true, BindingDirection.OneWay)] public partial IFileSystemChild? SelectedDetail { get; private set; }
        [ObservableProperty, Bindable(true, BindingDirection.OneWay)] public partial IFileSystemParent? SelectedTreeDetail { get; private set; }
        [ObservableProperty, Bindable(true, BindingDirection.OneWay)] public partial ScarletFile? SelectedFile { get; private set; }
        [ObservableProperty, Bindable(true, BindingDirection.OneWay)] public partial ScarletDirectory? SelectedDirectory { get; private set; }

        /// <summary>
        /// The content of the detail view, either the <see cref="SelectedContainer"/> or the search results.
        /// </summary>
        [ObservableProperty, Bindable(true, BindingDirection.OneWay)] public partial ReadOnlyObservableCollection<IFileSystemChild>? Entries { get; private set; }

        /// <summary>
        /// The selection in the detail view. Unlike the tree selection, it doesn't change the displayed folder.
        /// </summary>
        [ObservableProperty, Bindable(true, BindingDirection.TwoWay)] public partial IFileSystemChild? SelectedEntry { get; set; }

        /// <summary>
        /// What the inspector displays, the <see cref="SelectedEntry"/> if there is one, otherwise the <see cref="SelectedContainer"/>.
        /// </summary>
        [ObservableProperty, Bindable(true, BindingDirection.OneWay)] public partial IFileSystemInfo? Inspected { get; private set; }

        [ObservableProperty, Bindable(true, BindingDirection.OneWay)] public partial ScarletPermissions? InspectedPermissions { get; private set; }
        [ObservableProperty, Bindable(true, BindingDirection.OneWay)] public partial IReadOnlyList<FileAttributeFlag> InspectedAttributes { get; private set; } = [];
        [ObservableProperty, Bindable(true, BindingDirection.OneWay)] public partial IReadOnlyList<IFileSystemParent> Breadcrumbs { get; private set; } = [];

        [ObservableProperty, Bindable(true, BindingDirection.TwoWay)] public partial FileSystemViewMode ViewMode { get; set; }
        [ObservableProperty, Bindable(true, BindingDirection.TwoWay)] public partial bool IsPreviewVisible { get; set; } = true;
        [ObservableProperty, Bindable(true, BindingDirection.TwoWay)] public partial bool IsInspectorVisible { get; set; } = true;

        /// <summary>
        /// Searches the <see cref="SelectedContainer"/> and its subfolders. Supports the wildcards * and ?, see <see cref="FileSystemSearch.IsMatch"/>.
        /// </summary>
        [ObservableProperty, Bindable(true, BindingDirection.TwoWay)] public partial string SearchText { get; set; } = string.Empty;

        [ObservableProperty, Bindable(true, BindingDirection.OneWay)] public partial bool IsSearchActive { get; private set; }
        [ObservableProperty, Bindable(true, BindingDirection.OneWay)] public partial bool IsSearching { get; private set; }
        [ObservableProperty, Bindable(true, BindingDirection.OneWay)] public partial string SearchStatus { get; private set; } = string.Empty;

        [Bindable(true, BindingDirection.OneWay)] public FileSystemOptionsViewModel Options { get; }
        [Bindable(true, BindingDirection.OneWay)] public ReadOnlyObservableCollection<IFileSystemDrive> Items { get; }

        public FileSystemViewModel(
            IScheduler scheduler,
            IFileSystemViewModelFactory factory,
            FileSystemOptionsViewModel options)
        {
            ArgumentNullException.ThrowIfNull(factory);
            ArgumentNullException.ThrowIfNull(options);

            _scheduler = scheduler;
            _factory = factory;
            _items = new ObservableCollectionExtended<IFileSystemDrive>();
            _cache = new SourceCache<IFileSystemDrive, string>(vm => vm.Name);
            _searchResults = new ObservableCollectionExtended<IFileSystemChild>();
            _readOnlySearchResults = new ReadOnlyObservableCollection<IFileSystemChild>(_searchResults);
            _mapper = new FileSystemViewModelMapper(this, factory);

            var itemsSubscription = _cache.Connect()
                 .ObserveOn(scheduler)
                 .SortAndBind(_items, SortExpressionComparer<IFileSystemDrive>.Ascending(p => p.Name))
                 .Subscribe();

            _compositeDisposable = new CompositeDisposable(itemsSubscription);

            Options = options;
            Items = new ReadOnlyObservableCollection<IFileSystemDrive>(_items);
        }

        [RelayCommand(AllowConcurrentExecutions = false)]
        private Task Refresh(CancellationToken token)
        {
            return _mapper.Refresh(token);
        }

        [RelayCommand]
        private void ClearSearch()
        {
            SearchText = string.Empty;
        }

        [RelayCommand]
        private void SelectContainer(IFileSystemParent? container)
        {
            if (container is null || ReferenceEquals(container, SelectedContainer))
            {
                return;
            }

            // the tree only deselects the previous item, when the new one has a container already
            SelectedContainer?.IsSelected = false;
            container.IsSelected = true;
        }

        [RelayCommand]
        private void GoUp()
        {
            SelectContainer((SelectedContainer as IFileSystemChild)?.Parent);
        }

        /// <summary>
        /// Moves the tree selection to a folder of the detail view. Search results are separate instances, so the tree is walked by path.
        /// </summary>
        [RelayCommand]
        private async Task Open(IFileSystemChild? entry)
        {
            if (entry is not IFileSystemDirectory directory)
            {
                return;
            }

            IFileSystemParent? current = Items.FirstOrDefault(p => directory.FullName.StartsWith(p.FullName, StringComparison.OrdinalIgnoreCase));
            if (current is null)
            {
                return;
            }

            foreach (var segment in Path.GetRelativePath(current.FullName, directory.FullName).Split(Path.DirectorySeparatorChar))
            {
                if (!current.IsLoaded)
                {
                    await current.RefreshCommand.ExecuteAsync(null);

                    // the children reach the Items collection through the scheduler
                    await _scheduler.Yield();
                }

                current.IsToggled = true;

                var next = current.Items
                    .OfType<IFileSystemDirectory>()
                    .FirstOrDefault(p => string.Equals(p.Name, segment, StringComparison.OrdinalIgnoreCase));

                if (next is null)
                {
                    break;
                }

                current = next;
            }

            SelectContainer(current);
        }

        internal void OnContainerSelected(IFileSystemParent container)
        {
            _selectedEntryPath = null;
            SelectedContainer = container;
            SelectedEntry = null;
            Breadcrumbs = GetBreadcrumbs(container);

            SearchText = string.Empty;
            Entries = container.Items;
            Inspected = container;
        }

        private static List<IFileSystemParent> GetBreadcrumbs(IFileSystemParent container)
        {
            var result = new List<IFileSystemParent>();
            for (var current = container; current is not null; current = (current as IFileSystemChild)?.Parent)
            {
                result.Insert(0, current);
            }

            return result;
        }

        partial void OnSelectedEntryChanged(IFileSystemChild? value)
        {
            SelectedChild = value;
            SelectedDetail = value;
            SelectedFile = value as ScarletFile;
            SelectedDirectory = value as ScarletDirectory ?? SelectedContainer as ScarletDirectory;
            Inspected = value ?? (IFileSystemInfo?)SelectedContainer;

            // swapping the Entries clears the selection of the view, the path restores it, if the entry is still displayed
            if (value is not null)
            {
                _selectedEntryPath = value.FullName;
            }

            SelectedFile?.CheckHeaderCommand.Execute(null);
        }

        private void RestoreSelectedEntry(IEnumerable<IFileSystemChild> candidates)
        {
            if (SelectedEntry is null && _selectedEntryPath is not null)
            {
                var entry = candidates.FirstOrDefault(p => string.Equals(p.FullName, _selectedEntryPath, StringComparison.OrdinalIgnoreCase));
                if (entry is not null)
                {
                    SelectedEntry = entry;
                }
            }
        }

        async partial void OnInspectedChanged(IFileSystemInfo? value)
        {
            InspectedPermissions = null;
            InspectedAttributes = value is IFileSystemChild child
                ? _displayedAttributes.Select(p => new FileAttributeFlag(p.ToString(), child.Attributes.HasFlag(p))).ToList()
                : [];

            CancelAndDispose(ref _permissionsCancellation);
            if (value is not IFileSystemChild)
            {
                return;
            }

            var cancellation = new CancellationTokenSource();
            _permissionsCancellation = cancellation;

            try
            {
                var permissions = await _factory.GetPermissions(value.FullName, value.IsContainer, cancellation.Token);
                if (!cancellation.IsCancellationRequested)
                {
                    InspectedPermissions = permissions;
                }
            }
            catch (OperationCanceledException)
            {
                // another entry got selected in the meantime
            }
        }

        async partial void OnSearchTextChanged(string value)
        {
            CancelAndDispose(ref _searchCancellation);
            ClearSearchResults();

            var query = value.Trim();
            if (query.Length == 0 || SelectedContainer is null)
            {
                IsSearchActive = false;
                IsSearching = false;
                SearchStatus = string.Empty;
                Entries = SelectedContainer?.Items;

                if (Entries is not null)
                {
                    RestoreSelectedEntry(Entries);
                }

                return;
            }

            var cancellation = new CancellationTokenSource();
            _searchCancellation = cancellation;

            var scope = SelectedContainer;
            var skipped = 0;
            var isLimited = false;

            IsSearchActive = true;
            IsSearching = true;
            Entries = _readOnlySearchResults;
            UpdateSearchStatus(scope, skipped, isLimited);

            try
            {
                await foreach (var batch in _factory.Search(scope, query, Options.FileAttributes, Options.FolderAttributes, cancellation.Token))
                {
                    if (cancellation.IsCancellationRequested)
                    {
                        Dispose(batch.Items);
                        return;
                    }

                    var remaining = MaxSearchResults - _searchResults.Count;

                    _searchResults.AddRange(batch.Items.Take(remaining));
                    Dispose(batch.Items.Skip(remaining));
                    RestoreSelectedEntry(batch.Items.Take(remaining));

                    skipped += batch.IsSkipped ? 1 : 0;
                    isLimited = batch.Items.Count >= remaining;
                    UpdateSearchStatus(scope, skipped, isLimited);

                    if (isLimited)
                    {
                        break;
                    }
                }

                if (!cancellation.IsCancellationRequested)
                {
                    IsSearching = false;
                }
            }
            catch (OperationCanceledException)
            {
                // a newer search took over
            }
        }

        private void UpdateSearchStatus(IFileSystemParent scope, int skipped, bool isLimited)
        {
            var count = _searchResults.Count.ToString("N0", CultureInfo.CurrentCulture);
            var status = isLimited
                ? $"First {count} matches in {scope.Name} and subfolders"
                : $"{count} matches in {scope.Name} and subfolders";

            SearchStatus = skipped switch
            {
                0 => status,
                1 => status + ", 1 folder skipped (no access)",
                _ => status + $", {skipped} folders skipped (no access)",
            };
        }

        private void ClearSearchResults()
        {
            var results = _searchResults.ToList();
            _searchResults.Clear();

            Dispose(results);
        }

        private static void Dispose(IEnumerable<IFileSystemChild> children)
        {
            foreach (var child in children.OfType<IDisposable>())
            {
                child.Dispose();
            }
        }

        private static void CancelAndDispose(ref CancellationTokenSource? cancellation)
        {
            cancellation?.Cancel();
            cancellation?.Dispose();
            cancellation = null;
        }

        public void Dispose()
        {
            CancelAndDispose(ref _searchCancellation);
            CancelAndDispose(ref _permissionsCancellation);
            ClearSearchResults();
            _compositeDisposable.Dispose();
        }
    }
}
