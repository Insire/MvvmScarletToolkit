using MvvmScarletToolkit.Observables;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MvvmScarletToolkit
{
    // https://github.com/tom-englert/DataGridExtensions
    // http://dotnetpattern.com/wpf-datagrid-grouping
    public sealed class GroupingViewModel : BusinessViewModelListBase<GroupsViewModel>
    {
        private ConcurrentDictionary<string, GroupViewModel> _filterCollection;
        private int _maxGroupings;

        private readonly Type _type;

        public ICommand AddCommand { get; }

        public override ICommand RemoveCommand { get; }

        public GroupingViewModel(ICommandBuilder commandBuilder, Type type)
            : base(commandBuilder)
        {
            _type = type ?? throw new ArgumentNullException(nameof(type));
            _filterCollection = new ConcurrentDictionary<string, GroupViewModel>();

            AddCommand = commandBuilder.Create(Add, CanAdd)
                .WithBusyNotification(BusyStack)
                .WithSingleExecution(CommandManager)
                .WithCancellation()
                .Build();

            RemoveCommand = commandBuilder
                .Create<GroupsViewModel>(p => Remove(p), CanRemove)
                .WithSingleExecution(CommandManager)
                .WithBusyNotification(BusyStack)
                .WithCancellation()
                .Build();

            Messenger.Subscribe<ViewModelListBaseSelectionChanging<GroupViewModel>>((p) => _filterCollection.TryAdd(p.Content.Name, p.Content), (p) => !p.Sender.Equals(this) && !(p.Content is null));
            Messenger.Subscribe<ViewModelListBaseSelectionChanged<GroupViewModel>>((p) => _filterCollection.TryRemove(p.Content.Name, out var _), (p) => !p.Sender.Equals(this) && !(p.Content is null));
        }

        public override async Task Remove(GroupsViewModel item)
        {
            await base.Remove(item);

            if (!(item.SelectedItem is null))
            {
                _filterCollection.TryAdd(item.SelectedItem.Name, item.SelectedItem);
                Messenger.Publish(new GroupsViewModelRemoved(this, item));
            }
        }

        protected override Task RefreshInternal(CancellationToken token)
        {
            return Task.Run(() =>
            {
                _filterCollection = new ConcurrentDictionary<string, GroupViewModel>(_type
                        .GetProperties(BindingFlags.Instance | BindingFlags.Public)
                        .Where(p => p.CanRead)
                        .Where(p => p.GetGetMethod(true).IsPublic)
                        .Select(p => new GroupViewModel(CommandBuilder, p))
                        .Select(p => new KeyValuePair<string, GroupViewModel>(p.Name, p)));

                _maxGroupings = _filterCollection.Count;
            });
        }

        private async Task Add(CancellationToken token)
        {
            var result = new GroupsViewModel(CommandBuilder); // TODO
            await result.AddRange(_filterCollection.Values);

            await Add(result);
        }

        private bool CanAdd()
        {
            return !IsBusy
                && _filterCollection != null
                && _filterCollection.Count > 0
                && _maxGroupings > Count;
        }
    }
}
