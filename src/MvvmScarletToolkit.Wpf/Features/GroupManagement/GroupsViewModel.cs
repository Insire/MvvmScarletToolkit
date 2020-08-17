using MvvmScarletToolkit.Observables;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;

namespace MvvmScarletToolkit
{
    public sealed class GroupsViewModel : ViewModelListBase<GroupViewModel>
    {
        private readonly Func<ICollectionView> _collectionView;
        private readonly List<IDisposable> _disposeables;

        private bool _disposed;

        public GroupsViewModel(IScarletCommandBuilder commandBuilder, Func<ICollectionView> collectionView)
            : base(commandBuilder)
        {
            _collectionView = collectionView ?? throw new ArgumentNullException(nameof(collectionView));
            _disposeables = new List<IDisposable>
            {
                Messenger.Subscribe<ViewModelListBaseSelectionChanging<GroupViewModel>>(async (p) =>
                {
                    await Add(p.Content);
                }, (p) => !p.Sender.Equals(this) && !(p.Content is null)),

                Messenger.Subscribe<ViewModelListBaseSelectionChanged<GroupViewModel>>(async (p) =>
                {
                    await Remove(p.Content);
                    p.Content.Dispose();
                }, (p) => !p.Sender.Equals(this) && !(p.Content is null)),

                Messenger.Subscribe<GroupsViewModelRemoved>(async (p) =>
                {
                    var item = p.Content?.SelectedItem;
                    if (item != null)
                    {
                        await Add(item);
                    }
                }, (p) => !p.Sender.Equals(this) && !(p.Content is null) && !(p.Content.SelectedItem is null)),

                Messenger.Subscribe<ViewModelListBaseSelectionChanging<GroupViewModel>>((p) =>
                {
                    var view = _collectionView.Invoke();
                    view?.GroupDescriptions.Remove(p.Content.GroupDescription);
                }, (p) => p.Sender.Equals(this) && !(p.Content is null)),

                Messenger.Subscribe<ViewModelListBaseSelectionChanged<GroupViewModel>>((p) =>
                {
                    var view = _collectionView.Invoke();
                    view?.GroupDescriptions.Add(p.Content.GroupDescription);
                }, (p) => p.Sender.Equals(this) && !(p.Content is null)),

                Messenger.Subscribe<GroupsViewModelRemoved>(async p =>
                {
                    await Clear();
                }, p => p.Content.Equals(this))
            };
        }

        public override Task Clear(CancellationToken token)
        {
            var view = _collectionView.Invoke();
            view?.GroupDescriptions.Clear();

            return base.Clear(token);
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
