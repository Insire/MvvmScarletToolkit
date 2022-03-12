using CommunityToolkit.Mvvm.ComponentModel;
using System;

namespace MvvmScarletToolkit.Wpf
{
    public class ToastViewModel : ObservableObject, IToast
    {
        private bool _isRemoving;
        /// <summary>
        /// This is set to true immediately when the alloted display time runs out, but the toast is not removed immediately from the toast collection,
        /// so that there is time to animate its removal
        /// </summary>
        public bool IsRemoving
        {
            get { return _isRemoving; }
            set { SetProperty(ref _isRemoving, value); }
        }

        public string Title { get; }

        public string Body { get; }

        public Enum ToastType { get; }

        /// <summary>
        /// Whether the toast has to be closed by the user
        /// </summary>
        public bool IsPersistent { get; }

        public ToastViewModel(string title, string body, Enum toastType, bool isPersistent)
        {
            Title = title;
            Body = body;
            ToastType = toastType;
            IsPersistent = isPersistent;
        }
    }
}
