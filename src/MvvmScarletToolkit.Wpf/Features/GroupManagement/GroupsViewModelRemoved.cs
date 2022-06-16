using CommunityToolkit.Mvvm.Messaging.Messages;

namespace MvvmScarletToolkit
{
    public sealed class GroupsViewModelRemoved : ValueChangedMessage<GroupsViewModel>
    {
        public GroupsViewModelRemoved(GroupsViewModel content)
            : base(content)
        {
        }
    }
}
