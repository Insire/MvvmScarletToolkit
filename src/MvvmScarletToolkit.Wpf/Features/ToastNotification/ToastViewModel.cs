using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;
using System;
using System.Collections.ObjectModel;
using System.Windows.Input;
using System.Windows.Threading;

namespace MvvmScarletToolkit.Wpf
{
    public class ToastViewModel : ObservableObject
    {
        private readonly DispatcherTimer _timer;
        private readonly ObservableCollection<ToastViewModel> _toasts;

        public string Title { get; }

        public string Body { get; }

        public ToastType ToastType { get; }

        public bool IsPersistent { get; }

        public ICommand BeginDismissCommand { get; }

        public ICommand DismissCommand { get; }

        public ToastViewModel(ObservableCollection<ToastViewModel> toasts, string title, string body, ToastType toastType, bool isPersistent, TimeSpan displayTime)
        {
            _toasts = toasts ?? throw new ArgumentNullException(nameof(toasts));

            Title = title;
            Body = body;
            ToastType = toastType;
            IsPersistent = isPersistent;

            BeginDismissCommand = new RelayCommand(BeginDismissImpl);
            DismissCommand = new RelayCommand(DismissImpl);

            _timer = new DispatcherTimer
            {
                Interval = displayTime
            };
            _timer.Tick += OnTimerTick;
        }

        protected virtual void BeginDismissImpl()
        {
            _timer.Start();
        }

        protected virtual void DismissImpl()
        {
            OnToastClosing();
        }

        protected virtual void OnToastClosing()
        {
            _timer?.Stop();

            _toasts.Remove(this);
        }

        private void OnTimerTick(object? sender, EventArgs e)
        {
            OnToastClosing();
        }
    }
}
