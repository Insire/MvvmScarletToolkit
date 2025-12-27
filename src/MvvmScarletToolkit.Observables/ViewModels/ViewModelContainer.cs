using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;

namespace MvvmScarletToolkit
{
    /// <summary>
    /// Generic wrapper viewmodel to add binding support to any c# object
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public partial class ViewModelContainer<T> : ViewModelContainer
    {
        [ObservableProperty]
        public new partial T Value { get; set; }

        public ViewModelContainer(in T value, IMessenger messenger)
            : base(messenger)
        {
            Value = value;
        }
    }

    public abstract partial class ViewModelContainer : ObservableRecipient
    {
        [ObservableProperty]
        public partial object? Value { get; set; }

        [ObservableProperty]
        public partial string? DisplayName { get; set; }

        [ObservableProperty]
        [NotifyPropertyChangedRecipients]
        public partial bool IsSelected { get; set; }

        public ViewModelContainer(IMessenger messenger)
            : base(messenger)
        {
        }
    }
}
