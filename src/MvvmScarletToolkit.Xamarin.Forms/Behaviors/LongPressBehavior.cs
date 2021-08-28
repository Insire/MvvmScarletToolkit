using System;
using System.Threading;
using System.Windows.Input;
using Xamarin.Forms;

namespace MvvmScarletToolkit.Xamarin.Forms
{
    /// <summary>
    /// Behavior that triggers a command after a Button has been pressed for 1s
    /// </summary>
    // usage:
    // <Button.Behaviors>
    //   <mvvm:LongPressBehavior Command = "{Binding YourCommandProperty}" />
    // </ Button.Behaviors >
    public sealed class LongPressBehavior : BehaviorBase<Button>
    {
        private const int DurationInMilliseconds = 1000;

        public static readonly BindableProperty CommandProperty = BindableProperty.Create(
            nameof(Command),
            typeof(ICommand),
            typeof(LongPressBehavior),
            default(ICommand));

        public static readonly BindableProperty CommandParameterProperty = BindableProperty.Create(
            nameof(CommandParameter),
            typeof(object),
            typeof(LongPressBehavior));

        public object CommandParameter
        {
            get { return GetValue(CommandParameterProperty); }
            set { SetValue(CommandParameterProperty, value); }
        }

        public ICommand Command
        {
            get { return (ICommand)GetValue(CommandProperty); }
            set { SetValue(CommandProperty, value); }
        }

        private readonly object _syncRoot;
        private readonly int _duration;

        private Timer? _timer;

        private volatile bool _isReleased;

        /// <summary>
        /// Occurs when the associated button is long pressed.
        /// </summary>
        public event EventHandler? LongPressed;

        public LongPressBehavior()
        {
            _syncRoot = new object();
            _isReleased = true;
            _duration = DurationInMilliseconds;
        }

        public LongPressBehavior(int duration)
            : this()
        {
            _duration = duration;
        }

        protected override void OnAttachedTo(Button bindable)
        {
            base.OnAttachedTo(bindable);

            bindable.Pressed += Button_Pressed;
            bindable.Released += Button_Released;
        }

        protected override void OnDetachingFrom(Button bindable)
        {
            base.OnDetachingFrom(bindable);

            bindable.Pressed -= Button_Pressed;
            bindable.Released -= Button_Released;
        }

        private void Timer_Elapsed(object state)
        {
            DeInitializeTimer();

            if (_isReleased)
            {
                return;
            }

            Device.BeginInvokeOnMainThread(OnLongPressed);
        }

        /// <summary>
        /// DeInitializes and disposes the timer.
        /// </summary>
        private void DeInitializeTimer()
        {
            lock (_syncRoot)
            {
                if (_timer is null)
                {
                    return;
                }

                _timer.Change(Timeout.Infinite, Timeout.Infinite);
                _timer.Dispose();
                _timer = null;
            }
        }

        /// <summary>
        /// Initializes the timer.
        /// </summary>
        private void InitializeTimer()
        {
            lock (_syncRoot)
            {
                _timer = new Timer(Timer_Elapsed, null, _duration, Timeout.Infinite);
            }
        }

        private void Button_Pressed(object sender, EventArgs e)
        {
            _isReleased = false;

            InitializeTimer();
        }

        private void Button_Released(object sender, EventArgs e)
        {
            _isReleased = true;

            DeInitializeTimer();
        }

        private void OnLongPressed()
        {
            var handler = LongPressed;
            handler?.Invoke(this, EventArgs.Empty);

            if (Command?.CanExecute(CommandParameter) == true)
            {
                Command.Execute(CommandParameter);
            }
        }
    }
}
