using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace MvvmScarletToolkit
{
    public partial class SnakeView : ISnakeView
    {
        private readonly Dispatcher _dispatcher;
        private readonly IMessenger _messenger;

        private PropertyObserver<ISnakeManager> _propertyObserver;
        private FrameCounter _frameCounter;

        public ISnakeManager Manager
        {
            get { return (ISnakeManager)GetValue(ManagerhProperty); }
            set { SetValue(ManagerhProperty, value); }
        }

        public static readonly DependencyProperty ManagerhProperty = DependencyProperty.Register(
            "Manager",
            typeof(ISnakeManager),
            typeof(SnakeView),
            new PropertyMetadata(default(ISnakeManager)));

        public int FramesPerSecond
        {
            get { return (int)GetValue(FramesPerSecondProperty); }
            set { SetValue(FramesPerSecondProperty, value); }
        }

        public static readonly DependencyProperty FramesPerSecondProperty = DependencyProperty.Register(
            "FramesPerSecond",
            typeof(int),
            typeof(SnakeView),
            new PropertyMetadata(0));

        public bool IsFpsEnabled
        {
            get { return (bool)GetValue(IsFpsEnabledProperty); }
            set { SetValue(IsFpsEnabledProperty, value); }
        }

        public static readonly DependencyProperty IsFpsEnabledProperty = DependencyProperty.Register(
            "IsFpsEnabled",
            typeof(bool),
            typeof(SnakeView),
            new PropertyMetadata(false));

        public SnakeViewModel SnakeViewModel
        {
            get { return (SnakeViewModel)GetValue(SnakeViewModelProperty); }
            set { SetValue(SnakeViewModelProperty, value); }
        }

        public static readonly DependencyProperty SnakeViewModelProperty = DependencyProperty.Register(
            "SnakeViewModel",
            typeof(SnakeViewModel),
            typeof(SnakeView),
            new PropertyMetadata(default(SnakeViewModel)));

        public View View
        {
            get { return (View)GetValue(ViewProperty); }
            set { SetValue(ViewProperty, value); }
        }

        public static readonly DependencyProperty ViewProperty = DependencyProperty.Register(
            "View",
            typeof(View),
            typeof(SnakeView),
            new PropertyMetadata(default(View)));

        public ICommand ExitCommand
        {
            get { return (ICommand)GetValue(ExitCommandProperty); }
            set { SetValue(ExitCommandProperty, value); }
        }

        public static readonly DependencyProperty ExitCommandProperty = DependencyProperty.Register(
            "ExitCommand",
            typeof(ICommand),
            typeof(SnakeView),
            new PropertyMetadata(default(ICommand)));

        public IAsyncCommand ShowStartCommand
        {
            get { return (IAsyncCommand)GetValue(ShowStartCommandProperty); }
            set { SetValue(ShowStartCommandProperty, value); }
        }

        public static readonly DependencyProperty ShowStartCommandProperty = DependencyProperty.Register(
            "ShowStartCommand",
            typeof(IAsyncCommand),
            typeof(SnakeView),
            new PropertyMetadata(default(IAsyncCommand)));

        public IAsyncCommand ShowOptionsCommand
        {
            get { return (IAsyncCommand)GetValue(ShowOptionsCommandProperty); }
            set { SetValue(ShowOptionsCommandProperty, value); }
        }

        public static readonly DependencyProperty ShowOptionsCommandProperty = DependencyProperty.Register(
            "ShowOptionsCommand",
            typeof(IAsyncCommand),
            typeof(SnakeView),
            new PropertyMetadata(default(IAsyncCommand)));

        public IAsyncCommand ShowGameCommand
        {
            get { return (IAsyncCommand)GetValue(ShowGameCommandProperty); }
            set { SetValue(ShowGameCommandProperty, value); }
        }

        public static readonly DependencyProperty ShowGameCommandProperty = DependencyProperty.Register(
            "ShowGameCommand",
            typeof(IAsyncCommand),
            typeof(SnakeView),
            new PropertyMetadata(default(IAsyncCommand)));

        public SnakeView()
        {
            _dispatcher = Application.Current.Dispatcher;
            _messenger = new ScarletMessenger(new InternalLogger(), new DefaultMessageProxy());

            View = View.Start;
            ShowStartCommand = AsyncCommand.Create(ShowStart, CanShowStart);
            ShowOptionsCommand = AsyncCommand.Create(ShowOptions, CanShowOptions);
            ShowGameCommand = AsyncCommand.Create(ShowGame, CanShowGame);
            ExitCommand = new RelayCommand(Exit, CanExit);

            DataContext = this;

            if (SnakeViewModel == null)
                SnakeViewModel = new SnakeViewModel(new SnakeLogViewModel(_messenger));

            InitializeComponent();
        }

        private async Task ShowStart()
        {
            await Manager.Reset();
            View = View.Start;
        }

        private bool CanShowStart()
        {
            return View != View.Start;
        }

        private async Task ShowOptions()
        {
            await Manager.Reset();
            View = View.Options;
        }

        private bool CanShowOptions()
        {
            return View != View.Options;
        }

        private async Task ShowGame()
        {
            if (Manager != null)
                await Manager.Reset();

            View = View.Game;
            Manager = new SnakeEngine(SnakeViewModel.SelectedOption, _dispatcher, _messenger);
            SetupObserver();

            Keyboard.Focus(this);
            var play = Manager.Play();

            Initialize();

            await play;
        }

        private void SetupObserver()
        {
            _propertyObserver = new PropertyObserver<ISnakeManager>(Manager);
            _propertyObserver.RegisterHandler((o) => o.State, OnStateChanged);
        }

        private void OnStateChanged(ISnakeManager manager)
        {
            switch (manager.State)
            {
                case GameState.Fail:
                    View = View.Fail;
                    break;

                case GameState.None:
                    View = View.Start;
                    break;

                case GameState.Paused:
                case GameState.Running:
                    break;

                default:
                    throw new NotImplementedException(View + " is not implemented yet");
            }
        }

        private void DisposeObserver()
        {
            if (_propertyObserver == null)
                return;

            _propertyObserver.UnregisterHandler((o) => o.State);
            _propertyObserver = null;

        }

        private bool CanShowGame()
        {
            return View != View.Game;
        }

        private void Initialize()
        {
            _frameCounter = new FrameCounter(RenderNotification); // TODO enable + disable

            CompositionTarget.Rendering += CompositionTarget_Rendering;
        }

        private void Dispose()
        {
            CompositionTarget.Rendering -= CompositionTarget_Rendering;

            DisposeObserver();

            if (Manager != null)
            {
                Manager.Dispose();
                Manager = null;
            }

            if (_frameCounter != null)
            {
                _frameCounter.Dispose();
                _frameCounter = null;
            }
        }

        private void CompositionTarget_Rendering(object sender, EventArgs e)
        {
            Manager.Refresh();
            _frameCounter.UpdateCounter();
        }

        private void RenderNotification(object state)
        {
            if (!(state is int))
                return;

            _dispatcher.BeginInvoke(DispatcherPriority.Normal,
                new Action<int>(fps =>
                {
                    FramesPerSecond = fps;
                }), (int)state);
        }

        private void SnakeView_Unloaded(object sender, RoutedEventArgs e)
        {
            Dispose();
        }

        private void Exit()
        {
            Application.Current.MainWindow.Close();
        }

        private static bool CanExit()
        {
            return Application.Current != null && Application.Current.MainWindow != null;
        }
    }
}
