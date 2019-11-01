using MvvmScarletToolkit;
using MvvmScarletToolkit.Abstractions;
using MvvmScarletToolkit.Commands;
using MvvmScarletToolkit.Observables;
using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace DemoApp
{
    public sealed partial class SnakeView : ISnakeView
    {
        private readonly IScarletDispatcher _dispatcher;
        private readonly IScarletMessenger _messenger;
        private readonly IScarletCommandManager _commandManager;
        private readonly ICommandBuilder _commandBuilder;

        private PropertyObserver<ISnakeManager> _propertyObserver;
        private FrameCounter _frameCounter;

        public ISnakeManager Manager
        {
            get { return (ISnakeManager)GetValue(ManagerProperty); }
            set { SetValue(ManagerProperty, value); }
        }

        public static readonly DependencyProperty ManagerProperty = DependencyProperty.Register(
            nameof(Manager),
            typeof(ISnakeManager),
            typeof(SnakeView),
            new PropertyMetadata(default(ISnakeManager)));

        public int FramesPerSecond
        {
            get { return (int)GetValue(FramesPerSecondProperty); }
            set { SetValue(FramesPerSecondProperty, value); }
        }

        public static readonly DependencyProperty FramesPerSecondProperty = DependencyProperty.Register(
            nameof(FramesPerSecond),
            typeof(int),
            typeof(SnakeView),
            new PropertyMetadata(0));

        public bool IsDebug
        {
            get { return (bool)GetValue(IsDebugProperty); }
            set { SetValue(IsDebugProperty, value); }
        }

        public static readonly DependencyProperty IsDebugProperty = DependencyProperty.Register(
            nameof(IsDebug),
            typeof(bool),
            typeof(SnakeView), new PropertyMetadata(false));

        public bool IsFpsEnabled
        {
            get { return (bool)GetValue(IsFpsEnabledProperty); }
            set { SetValue(IsFpsEnabledProperty, value); }
        }

        public static readonly DependencyProperty IsFpsEnabledProperty = DependencyProperty.Register(
            nameof(IsFpsEnabled),
            typeof(bool),
            typeof(SnakeView),
            new PropertyMetadata(false, new PropertyChangedCallback(OnIsFpsEnabledChanged)));

        public SnakeViewModel SnakeViewModel
        {
            get { return (SnakeViewModel)GetValue(SnakeViewModelProperty); }
            set { SetValue(SnakeViewModelProperty, value); }
        }

        public static readonly DependencyProperty SnakeViewModelProperty = DependencyProperty.Register(
            nameof(SnakeViewModel),
            typeof(SnakeViewModel),
            typeof(SnakeView),
            new PropertyMetadata(default(SnakeViewModel)));

        public View View
        {
            get { return (View)GetValue(ViewProperty); }
            set { SetValue(ViewProperty, value); }
        }

        public static readonly DependencyProperty ViewProperty = DependencyProperty.Register(
            nameof(View),
            typeof(View),
            typeof(SnakeView),
            new PropertyMetadata(default(View)));

        public ICommand ExitCommand
        {
            get { return (ICommand)GetValue(ExitCommandProperty); }
            set { SetValue(ExitCommandProperty, value); }
        }

        public static readonly DependencyProperty ExitCommandProperty = DependencyProperty.Register(
            nameof(ExitCommand),
            typeof(ICommand),
            typeof(SnakeView),
            new PropertyMetadata(default(ICommand)));

        public IConcurrentCommand ShowStartCommand
        {
            get { return (IConcurrentCommand)GetValue(ShowStartCommandProperty); }
            set { SetValue(ShowStartCommandProperty, value); }
        }

        public static readonly DependencyProperty ShowStartCommandProperty = DependencyProperty.Register(
            nameof(ShowStartCommand),
            typeof(IConcurrentCommand),
            typeof(SnakeView),
            new PropertyMetadata(default(IConcurrentCommand)));

        public IConcurrentCommand ShowOptionsCommand
        {
            get { return (IConcurrentCommand)GetValue(ShowOptionsCommandProperty); }
            set { SetValue(ShowOptionsCommandProperty, value); }
        }

        public static readonly DependencyProperty ShowOptionsCommandProperty = DependencyProperty.Register(
            nameof(ShowOptionsCommand),
            typeof(IConcurrentCommand),
            typeof(SnakeView),
            new PropertyMetadata(default(IConcurrentCommand)));

        public IConcurrentCommand ShowGameCommand
        {
            get { return (IConcurrentCommand)GetValue(ShowGameCommandProperty); }
            set { SetValue(ShowGameCommandProperty, value); }
        }

        public static readonly DependencyProperty ShowGameCommandProperty = DependencyProperty.Register(
            nameof(ShowGameCommand),
            typeof(IConcurrentCommand),
            typeof(SnakeView),
            new PropertyMetadata(default(IConcurrentCommand)));

        private static void OnIsFpsEnabledChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (!(d is SnakeView snakeView))
            {
                return;
            }

            if (!(e.NewValue is bool enableFps))
            {
                return;
            }

            if (enableFps)
            {
                snakeView.SetupFpsCounter();
            }
            else
            {
                snakeView.DisposeFpsCounter();
            }
        }

        public SnakeView()
        {
            _commandManager = new ScarletCommandManager();
            _dispatcher = ScarletDispatcher.Default;
            _messenger = new ScarletMessenger(new ScarletMessageProxy());
            _commandBuilder = new CommandBuilder(_dispatcher, _commandManager, _messenger, ScarletExitService.Default, ScarletWeakEventManager.Default, (lambda) => new BusyStack(lambda, _dispatcher));

            View = View.Start;
            ShowStartCommand = _commandBuilder.Create(ShowStart, CanShowStart).Build();
            ShowOptionsCommand = _commandBuilder.Create(ShowOptions, CanShowOptions).Build();
            ShowGameCommand = _commandBuilder.Create(ShowGame, CanShowGame).Build();
            ExitCommand = new RelayCommand(_commandManager, Exit, CanExit);

            DataContext = this;

            if (SnakeViewModel == null)
            {
                SnakeViewModel = new SnakeViewModel(new SnakeLogViewModel(_messenger));
            }

            InitializeComponent();
        }

        private async Task ShowStart()
        {
            await Manager.Reset().ConfigureAwait(false);
            View = View.Start;
        }

        private bool CanShowStart()
        {
            return View != View.Start;
        }

        private async Task ShowOptions()
        {
            await Manager.Reset().ConfigureAwait(false);
            View = View.Options;
        }

        private bool CanShowOptions()
        {
            return View != View.Options;
        }

        private async Task ShowGame()
        {
            if (Manager != null)
            {
                await Manager.Reset().ConfigureAwait(false);
            }

            Manager = new SnakeEngine(SnakeViewModel.SelectedOption, _dispatcher, _messenger, _commandBuilder, _commandManager);
            SetupObserver();

            Keyboard.Focus(this);
            var play = Manager.Play();

            Initialize();

            await play;
        }

        private void SetupObserver()
        {
            if (_propertyObserver != null)
            {
                return;
            }

            _propertyObserver = new PropertyObserver<ISnakeManager>(Manager);
            _propertyObserver.RegisterHandler((o) => o.State, OnStateChanged);
        }

        private void DisposeObserver()
        {
            if (_propertyObserver == null)
            {
                return;
            }

            _propertyObserver.UnregisterHandler((o) => o.State);
            _propertyObserver = null;
        }

        private void OnStateChanged(ISnakeManager manager)
        {
            switch (manager.State)
            {
                case GameState.Fail:
                    View = View.Fail;
                    //DisposeObserver();
                    break;

                case GameState.None:
                    View = View.Start;
                    break;

                case GameState.Paused:
                case GameState.Running:
                    View = View.Game;
                    break;

                default:
                    throw new NotImplementedException(View + " is not implemented yet");
            }
        }

        private void SetupFpsCounter()
        {
            if (_frameCounter != null)
            {
                return;
            }

            _frameCounter = new FrameCounter(RenderNotification);
        }

        private void DisposeFpsCounter()
        {
            if (_frameCounter == null)
            {
                return;
            }

            _frameCounter.Dispose();
            _frameCounter = null;
        }

        private bool CanShowGame()
        {
            return View != View.Game;
        }

        private void Initialize()
        {
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

            DisposeFpsCounter();
        }

        private void CompositionTarget_Rendering(object sender, EventArgs e)
        {
            Manager.Refresh();
            _frameCounter?.UpdateCounter();
        }

        private void RenderNotification(object state)
        {
            if (!(state is int))
            {
                return;
            }

            _dispatcher.Invoke(() => FramesPerSecond = (int)state);
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
            return Application.Current?.MainWindow != null;
        }
    }
}
