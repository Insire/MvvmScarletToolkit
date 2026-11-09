using CommunityToolkit.Mvvm.Messaging;
using MvvmScarletToolkit.Observables;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace MvvmScarletToolkit
{
    // holds group descriptions, so that one can be selected and applied to a collection
    [DebuggerDisplay("{" + nameof(GetDebuggerDisplay) + "(),nq}")]
    public sealed class GroupsViewModel : ViewModelListBase<GroupViewModel>
    {
        private readonly Func<ICollectionView> _collectionViewFactory;
        private readonly GroupingViewModel? _owner;

        public GroupsViewModel(IScarletCommandBuilder commandBuilder, Func<ICollectionView> collectionViewFactory)
            : this(commandBuilder, collectionViewFactory, null)
        {
        }

        internal GroupsViewModel(IScarletCommandBuilder commandBuilder, Func<ICollectionView> collectionViewFactory, GroupingViewModel? owner)
            : base(commandBuilder)
        {
            _collectionViewFactory = collectionViewFactory ?? throw new ArgumentNullException(nameof(collectionViewFactory));
            _owner = owner;

            Messenger.Register<GroupsViewModel, ViewModelListBaseSelectionChanging<GroupViewModel>>(this, async (r, m) => await r.OnSelectionChanging(m));
            Messenger.Register<GroupsViewModel, ViewModelListBaseSelectionChanged<GroupViewModel>>(this, async (r, m) => await r.OnSelectionChanged(m));
            Messenger.Register<GroupsViewModel, GroupsViewModelRemoved>(this, async (r, m) => await r.OnGroupsRemoved(m));
        }

        /// <summary>
        /// Whether <paramref name="group"/> belongs to the same <see cref="GroupingViewModel"/> as this instance
        /// </summary>
        private bool IsSibling([NotNullWhen(true)] GroupViewModel? group)
        {
            return group is not null && ReferenceEquals(group.Owner, _owner);
        }

        private async Task OnSelectionChanging(ViewModelListBaseSelectionChanging<GroupViewModel> message)
        {
            var group = message.Value;
            if (!IsSibling(group))
            {
                return;
            }

            if (ReferenceEquals(message.Sender, this))
            {
                // this instance selection is changing
                // => remove the previous value from the view
                var view = _collectionViewFactory.Invoke();
                await Dispatcher.Invoke(() => view.GroupDescriptions.Remove(group.GroupDescription));
            }
            else
            {
                // another instance selection is changing
                // => add the last value to the possible selections
                await Add(group);
            }
        }

        private async Task OnSelectionChanged(ViewModelListBaseSelectionChanged<GroupViewModel> message)
        {
            var group = message.Value;
            if (!IsSibling(group))
            {
                return;
            }

            if (ReferenceEquals(message.Sender, this))
            {
                // this instance selection changed
                // => add the current value to the view
                var view = _collectionViewFactory.Invoke();
                await Dispatcher.Invoke(() => view.GroupDescriptions.Add(group.GroupDescription));
            }
            else
            {
                // another instance selection changed
                // => remove the current value from the possible selections
                // the group is now selected by the sender, so it has to stay usable
                await Remove(group);
            }
        }

        private async Task OnGroupsRemoved(GroupsViewModelRemoved message)
        {
            var removed = message.Value;
            if (removed.SelectedItem is null || ReferenceEquals(removed, this) || !ReferenceEquals(removed._owner, _owner))
            {
                return;
            }

            // another instance was removed, so if the selection was valid, add it back to the pool of possible selections
            await Add(removed.SelectedItem);
        }

        public override async Task Clear(CancellationToken token)
        {
            var view = _collectionViewFactory.Invoke();
            await Dispatcher.Invoke(() => view.GroupDescriptions.Clear());

            await base.Clear(token);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                Messenger.UnregisterAll(this);
            }

            base.Dispose(disposing);
        }

        private string GetDebuggerDisplay()
        {
            return $"{nameof(GroupsViewModel)}: {SelectedItem?.Name ?? "none"}";
        }
    }
}
