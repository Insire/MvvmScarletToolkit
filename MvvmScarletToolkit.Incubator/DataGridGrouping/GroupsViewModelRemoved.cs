using MvvmScarletToolkit.Observables;

namespace MvvmScarletToolkit
{
    public sealed class GroupsViewModelRemoved : GenericScarletMessage<GroupsViewModel>
    {
        public GroupsViewModelRemoved(object sender, GroupsViewModel content)
            : base(sender, content)
        {
        }
    }
}
