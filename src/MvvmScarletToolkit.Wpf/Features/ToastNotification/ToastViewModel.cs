using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;
using System;
using System.Windows.Input;
using System.Windows.Threading;

namespace MvvmScarletToolkit.Wpf
{
    public class ToastViewModel : ObservableObject
    {
        private readonly DispatcherTimer _timer;
        private readonly ToastService _toastService;

        public string Title { get; }

        public string Body { get; }

        public ToastType ToastType { get; }

        public bool IsPersistent { get; }

        public ICommand BeginDismissCommand { get; }

        public ICommand DismissCommand { get; }

        public ToastViewModel(ToastService toastService, string title, string body, ToastType toastType, bool isPersistent, TimeSpan displayTime)
        {
            _toastService = toastService ?? throw new ArgumentNullException(nameof(toastService));

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

        protected virtual async void OnToastClosing()
        {
            _timer?.Stop();

            await _toastService.Remove(this).ConfigureAwait(false);
        }

        private void OnTimerTick(object? sender, EventArgs e)
        {
            OnToastClosing();
        }
    }
}
