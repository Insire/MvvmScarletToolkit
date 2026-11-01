using Avalonia.Controls.Notifications;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace MvvmScarletToolkit.Avalonia.Samples.Features
{
    /// <summary>
    /// The toast service of the WPF package has no Avalonia counterpart, Avalonia brings its own notifications instead
    /// </summary>
    public sealed partial class ToastsViewModel : ObservableObject
    {
        private readonly DesktopToastManager _toastManager;

        public IReadOnlyList<NotificationType> ToastTypes { get; } = Enum.GetValues<NotificationType>();

        [ObservableProperty]
        public partial NotificationType ToastType { get; set; }

        [ObservableProperty]
        public partial string Title { get; set; }

        [ObservableProperty]
        public partial string Body { get; set; }

        [ObservableProperty]
        public partial bool Persist { get; set; }

        public ToastsViewModel(DesktopToastManager toastManager)
        {
            _toastManager = toastManager;

            Title = "Toast-Title";
            Body = "Toast-Body";
            ToastType = NotificationType.Success;
        }

        [RelayCommand]
        private void ShowToast()
        {
            // an expiration of zero keeps the notification open until it is closed
            _toastManager.Show(new Notification(Title, Body, ToastType, Persist ? TimeSpan.Zero : null));
        }

        [RelayCommand]
        private void ShowManyToasts()
        {
            for (var i = 0; i < 100; i++)
            {
                ShowToast();
            }
        }
    }
}
