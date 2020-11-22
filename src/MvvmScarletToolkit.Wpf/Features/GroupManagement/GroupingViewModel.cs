using Microsoft.Toolkit.Mvvm.Messaging;
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
    // manages
    // https://github.com/tom-englert/DataGridExtensions
    // http://dotnetpattern.com/wpf-datagrid-grouping
    [System.Diagnostics.CodeAnalysis.SuppressMessage("IDisposableAnalyzers.Correctness", "IDISP001:Dispose created.", Justification = "Class is a container class and owns all instances")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("IDisposableAnalyzers.Correctness", "IDISP007:Don't dispose injected.", Justification = "Class is a container class and owns all instances")]
    public sealed class GroupingViewModel : BusinessViewModelListBase<GroupsViewModel>
    {
        private readonly ConcurrentDictionary<string, GroupViewModel> _filterCollection;
        private readonly Func<ICollectionView> _collectionViewFactory;
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
                .Create<GroupsViewModel>((p, t) => Remove(p, t), (p) => CanRemove(p))
                .WithSingleExecution()
                .WithBusyNotification(BusyStack)
                .WithCancellation()
                .Build();

            Messenger.Register<GroupingViewModel, ViewModelListBaseSelectionChanging<GroupViewModel>>(this, (r, m) =>
            {
                if (m.Value is null)
                {
                    return;
                }

                r._filterCollection.TryAdd(m.Value.Name, m.Value);
            });

            Messenger.Register<GroupingViewModel, ViewModelListBaseSelectionChanged<GroupViewModel>>(this, (r, m) =>
            {
                if (m.Value is null)
                {
                    return;
                }

                r._filterCollection.TryRemove(m.Value.Name, out var _);
            });
        }

        public override async Task Remove(GroupsViewModel item, CancellationToken token)
        {
            await base.Remove(item, token).ConfigureAwait(false);
            Messenger.Send(new GroupsViewModelRemoved(item));

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
                    .Where(p => p.CanRead && p.GetGetMethod(true)?.IsPublic == true)
                    .Select(p => new GroupViewModel(CommandBuilder, p))
                    .Select(p => new KeyValuePair<string, GroupViewModel>(p.Name, p))
                    .ForEach(p => _filterCollection.TryAdd(p.Key, p.Value));

                _maxGroupings = _filterCollection.Count;
            });
        }

        private async Task Add(CancellationToken token)
        {
            var result = new GroupsViewModel(CommandBuilder, _collectionViewFactory);
            await result.AddRange(_filterCollection.Values).ConfigureAwait(false);

            await Add(result).ConfigureAwait(false);
        }

        private bool CanAdd()
        {
            return !_disposed
                && !IsBusy
                && _filterCollection?.Count > 0
                && _maxGroupings > Count;
        }

        public override async Task Clear(CancellationToken token)
        {
            for (var i = 0; i < _items.Count; i++)
            {
                var item = _items[i];
                item.Dispose();
            }

            await base.Clear(token).ConfigureAwait(false);
        }

        protected override void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
            }

            base.Dispose(disposing);
            _disposed = true;
        }
    }
}
