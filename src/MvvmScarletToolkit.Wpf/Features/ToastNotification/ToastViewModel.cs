using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;
using System;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Threading;

namespace MvvmScarletToolkit.Wpf
{
    public class ToastViewModel : ObservableObject
    {
        private readonly DispatcherTimer _timer;
        private readonly ToastService _toastService;

        private bool _isRemoving;
        /// <summary>
        /// This is set to true immediately when the alloted display time runs out, but the toast is not removed immediately from the toast collection,
        /// so that there is time to animate its removal
        /// </summary>
        public bool IsRemoving
        {
            get { return _isRemoving; }
            private set { SetProperty(ref _isRemoving, value); }
        }

        public string Title { get; }

        public string Body { get; }

        public Enum ToastType { get; }

        /// <summary>
        /// Whether the toast has to be closed by the user
        /// </summary>
        public bool IsPersistent { get; }

        /// <summary>
        /// will close and remove the toast after its configured display time
        /// </summary>
        public ICommand BeginDismissCommand { get; }

        /// <summary>
        /// will close and remove the toast immediately
        /// </summary>
        public ICommand DismissCommand { get; }

        public ToastViewModel(ToastService toastService, string title, string body, Enum toastType, bool isPersistent, TimeSpan displayTime)
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
            if (!IsPersistent)
            {
                _timer.Start();
            }
        }

        protected virtual void DismissImpl()
        {
            OnToastClosing();
        }

        protected virtual async void OnToastClosing()
        {
            IsRemoving = true;

            if (!IsPersistent)
            {
                _timer?.Stop();
                await Task.Delay(_toastService.ToastCloseDelay).ConfigureAwait(false); // we need to wait a bit, since i'm not aware of a good way to animate removal
            }

            await _toastService.Remove(this).ConfigureAwait(false);
        }

        private void OnTimerTick(object? sender, EventArgs e)
        {
            OnToastClosing();
        }
    }
}
