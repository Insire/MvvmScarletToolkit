using MvvmScarletToolkit.Observables;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MvvmScarletToolkit
{
    // https://github.com/tom-englert/DataGridExtensions
    // http://dotnetpattern.com/wpf-datagrid-grouping
    [System.Diagnostics.CodeAnalysis.SuppressMessage("IDisposableAnalyzers.Correctness", "IDISP001:Dispose created.", Justification = "Class is a container class and owns all instances")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("IDisposableAnalyzers.Correctness", "IDISP007:Don't dispose injected.", Justification = "Class is a container class and owns all instances")]
    public sealed class GroupingViewModel : BusinessViewModelListBase<GroupsViewModel>
    {
        private readonly ConcurrentDictionary<string, GroupViewModel> _filterCollection;
        private readonly Func<ICollectionView> _collectionViewFactory;
        private readonly List<IDisposable> _disposeables;
        private readonly Type _type;

        private bool _disposed;
        private int _maxGroupings;

        public ICommand AddCommand { get; }

        public override ICommand RemoveCommand { get; }

        public GroupingViewModel(IScarletCommandBuilder commandBuilder, Func<ICollectionView> collectionViewFactory, Type type)
            : base(commandBuilder)
        {
            _type = type ?? throw new ArgumentNullException(nameof(type));
            _filterCollection = new ConcurrentDictionary<string, GroupViewModel>();
            _collectionViewFactory = collectionViewFactory ?? throw new ArgumentNullException(nameof(collectionViewFactory));

            AddCommand = commandBuilder.Create(Add, CanAdd)
                .WithBusyNotification(BusyStack)
                .WithSingleExecution()
                .WithCancellation()
                .Build();

            RemoveCommand = commandBuilder
                .Create<GroupsViewModel>(p => Remove(p), CanRemove)
                .WithSingleExecution()
                .WithBusyNotification(BusyStack)
                .WithCancellation()
                .Build();

            _disposeables = new List<IDisposable>
            {
                Messenger.Subscribe<ViewModelListBaseSelectionChanging<GroupViewModel>>((p) => _filterCollection.TryAdd(p.Content.Name, p.Content), (p) => !p.Sender.Equals(this) && !(p.Content is null)),
                Messenger.Subscribe<ViewModelListBaseSelectionChanged<GroupViewModel>>((p) => _filterCollection.TryRemove(p.Content.Name, out var _), (p) => !p.Sender.Equals(this) && !(p.Content is null))
            };
        }

        public override async Task Remove(GroupsViewModel item, CancellationToken token)
        {
            await base.Remove(item, token);
            Messenger.Publish(new GroupsViewModelRemoved(this, item));

            if (!(item.SelectedItem is null))
            {
                _filterCollection.TryAdd(item.SelectedItem.Name, item.SelectedItem);
            }
        }

        protected override Task RefreshInternal(CancellationToken token)
        {
            _filterCollection.Clear();

            return Task.Run(() =>
            {
                _type
                    .GetProperties(BindingFlags.Instance | BindingFlags.Public)
                    .Where(p => p.CanRead)
                    .Where(p => p.GetGetMethod(true)?.IsPublic == true)
                    .Select(p => new GroupViewModel(CommandBuilder, p))
                    .Select(p => new KeyValuePair<string, GroupViewModel>(p.Name, p))
                    .ForEach(p => _filterCollection.TryAdd(p.Key, p.Value));

                _maxGroupings = _filterCollection.Count;
            });
        }

        private async Task Add(CancellationToken token)
        {
            var result = new GroupsViewModel(CommandBuilder, _collectionViewFactory);
            await result.AddRange(_filterCollection.Values);

            await Add(result);
        }

        private bool CanAdd()
        {
            return !_disposed
                && !IsBusy
                && _filterCollection != null
                && _filterCollection.Count > 0
                && _maxGroupings > Count;
        }

        public override async Task Clear(CancellationToken token)
        {
            for (var i = 0; i < _items.Count; i++)
            {
                var item = _items[i];
                item.Dispose();
            }

            await base.Clear(token);
        }

        protected override void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                _disposeables.ForEach(p => p.Dispose());
                _disposeables.Clear();
            }

            base.Dispose(disposing);
            _disposed = true;
        }
    }
}
