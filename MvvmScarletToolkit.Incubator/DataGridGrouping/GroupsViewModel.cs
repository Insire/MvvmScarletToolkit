using MvvmScarletToolkit.Observables;

namespace MvvmScarletToolkit
{
    public sealed class GroupsViewModel : ViewModelListBase<GroupViewModel>
    {
        public GroupsViewModel(ICommandBuilder commandBuilder)
            : base(commandBuilder)
        {
            Messenger.Subscribe<ViewModelListBaseSelectionChanging<GroupViewModel>>(async (p) =>
            {
                await Add(p.Content);
            }, (p) => !p.Sender.Equals(this) && !(p.Content is null));

            Messenger.Subscribe<ViewModelListBaseSelectionChanged<GroupViewModel>>(async (p) =>
            {
                await Remove(p.Content);
            }, (p) => !p.Sender.Equals(this) && !(p.Content is null));

            Messenger.Subscribe<GroupsViewModelRemoved>(async (p) =>
            {
                await Add(p.Content.SelectedItem);
            }, (p) => !p.Sender.Equals(this) && !(p.Content is null));
        }
    }
}
