using System;
using System.Windows;

namespace MvvmScarletToolkit
{
    public class Scene : ObservableObject
    {
        private FrameworkElement _content;
        public FrameworkElement Content
        {
            get { return _content; }
            set { SetValue(ref _content, value); }
        }

        private string _displayName;
        public string DisplayName
        {
            get { return _displayName; }
            set { SetValue(ref _displayName, value); }
        }
    }

    public class Scene<T> : Scene
        where T : class
    {
        public Func<ObservableObject> GetDataContext { get; set; }
        public Func<string> GetDisplayName { get; set; }

        private BusyStack _busyStack;
        public BusyStack BusyStack
        {
            get { return _busyStack; }
            private set { SetValue(ref _busyStack, value); }
        }

        private bool _isBusy;
        /// <summary>
        /// Indicates if there is an operation running.
        /// Modified by adding <see cref="BusyToken"/> to the <see cref="BusyStack"/> property
        /// </summary>
        public bool IsBusy
        {
            get { return _isBusy; }
            private set { SetValue(ref _isBusy, value); }
        }

        private bool _isSelected;
        public bool IsSelected
        {
            get { return _isSelected; }
            set { SetValue(ref _isSelected, value, OnChanged: UpdateDataContext); }
        }

        public Scene()
        {
            BusyStack = new BusyStack((hasItems) => IsBusy = hasItems);
        }

        private void UpdateDataContext()
        {
            if (Content == null || !IsSelected || GetDataContext == null)
                return;

            // while fetching the dataconext, we will switch IsBusy accordingly
            using (var token = BusyStack.GetToken())
            {
                var currentContext = Content.DataContext as T;
                var newContext = GetDataContext.Invoke();

                if (currentContext == null && newContext == null)
                    return;

                if (currentContext == newContext)
                    return;

                Content.DataContext = newContext;
            }
        }
    }
}
