using Microsoft.Toolkit.Mvvm.Messaging;
using MvvmScarletToolkit.Observables;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace MvvmScarletToolkit
{
    // holds group descriptions, so that one can be selected and applied to a collection
    [DebuggerDisplay("{" + nameof(GetDebuggerDisplay) + "(),nq}")]
    public sealed class GroupsViewModel : ViewModelListBase<GroupViewModel>
    {
        private readonly Func<ICollectionView> _collectionViewFactory;

        public GroupsViewModel(IScarletCommandBuilder commandBuilder, Func<ICollectionView> collectionViewFactory)
            : base(commandBuilder)
        {
            _collectionViewFactory = collectionViewFactory ?? throw new ArgumentNullException(nameof(collectionViewFactory));

            Messenger.Register<GroupsViewModel, ViewModelListBaseSelectionChanging<GroupViewModel>>(this, async (r, m) =>
            {
                if (m.Value is null)
                {
                    return;
                }

                if (ReferenceEquals(m.Sender, r))
                {
                    //Debug.WriteLine($"1 this instance({r.GetDebuggerDisplay()}) selection is changing => remove the previous value({m.Value.Name}) from the view");
                    // this instance selection is changing
                    // => remove the previous value from the view
                    var view = _collectionViewFactory.Invoke();
                    await Dispatcher.Invoke(() => view?.GroupDescriptions.Remove(m.Value.GroupDescription));
                }
                else
                {
                    //Debug.WriteLine($"2 another instance selection is changing => add the last value({m.Value.Name}) to the possible selections");
                    // another instance selection is changing
                    // => add the last value to the possible selections
                    await r.Add(m.Value);
                }
            });

            Messenger.Register<GroupsViewModel, ViewModelListBaseSelectionChanged<GroupViewModel>>(this, async (r, m) =>
            {
                if (m.Value is null)
                {
                    return;
                }

                if (ReferenceEquals(m.Sender, r))
                {
                    //Debug.WriteLine($"3 this instance({r.GetDebuggerDisplay()}) selection changed => add the current value({m.Value.Name}) to the view");
                    // this instance selection changed
                    // => add the current value to the view
                    var view = _collectionViewFactory.Invoke();
                    await Dispatcher.Invoke(() => view?.GroupDescriptions.Add(m.Value.GroupDescription));
                }
                else
                {
                    //Debug.WriteLine($"4 another instance selection changed =>  remove the current value({m.Value.Name}) from the possible selections");
                    // another instance selection changed
                    // => remove the current value from the possible selections
                    await r.Remove(m.Value);
                    m.Value.Dispose();
                }
            });

            Messenger.Register<GroupsViewModel, GroupsViewModelRemoved>(this, async (r, m) =>
            {
                if (m?.Value?.SelectedItem is null)
                {
                    return;
                }

                // another instance was removed, so if the selection was valid, add it back to the pool of possible selections
                await r.Add(m.Value.SelectedItem);
            });
        }

        public override async Task Clear(CancellationToken token)
        {
            var view = _collectionViewFactory.Invoke();
            await Dispatcher.Invoke(() => view?.GroupDescriptions.Clear());

            await base.Clear(token);
        }

        private string GetDebuggerDisplay()
        {
            return $"{nameof(GroupsViewModel)}: {SelectedItem?.Name ?? "none"}";
        }
    }
}
