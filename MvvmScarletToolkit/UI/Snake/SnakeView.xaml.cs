using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace MvvmScarletToolkit
{
    public partial class SnakeView
    {
        private readonly Dispatcher _dispatcher;
        private readonly ILogger _log;

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

        public ICommand ShowStartCommand
        {
            get { return (ICommand)GetValue(ShowStartCommandProperty); }
            set { SetValue(ShowStartCommandProperty, value); }
        }

        public static readonly DependencyProperty ShowStartCommandProperty = DependencyProperty.Register(
            "ShowStartCommand",
            typeof(ICommand),
            typeof(SnakeView),
            new PropertyMetadata(default(ICommand)));

        public ICommand ShowOptionsCommand
        {
            get { return (ICommand)GetValue(ShowOptionsCommandProperty); }
            set { SetValue(ShowOptionsCommandProperty, value); }
        }

        public static readonly DependencyProperty ShowOptionsCommandProperty = DependencyProperty.Register(
            "ShowOptionsCommand",
            typeof(ICommand),
            typeof(SnakeView),
            new PropertyMetadata(default(ICommand)));

        public ICommand ShowGameCommand
        {
            get { return (ICommand)GetValue(ShowGameCommandProperty); }
            set { SetValue(ShowGameCommandProperty, value); }
        }

        public static readonly DependencyProperty ShowGameCommandProperty = DependencyProperty.Register(
            "ShowGameCommand",
            typeof(ICommand),
            typeof(SnakeView),
            new PropertyMetadata(default(ICommand)));

        private void ShowStart()
        {
            View = View.Start;
        }

        private bool CanShowStart()
        {
            return View != View.Start;
        }

        private void ShowOptions()
        {
            View = View.Options;
        }

        private bool CanShowOptions()
        {
            return View != View.Options;
        }

        private void ShowGame()
        {
            View = View.Game;
            Manager = new SnakeManager(SnakeViewModel.SelectedOption, _dispatcher, _log);

            Initialize();
        }

        private bool CanShowGame()
        {
            return View != View.Game;
        }

        public SnakeView()
        {
            _dispatcher = Application.Current.Dispatcher;
            _log = new InternalLogger();

            View = View.Start;
            ShowStartCommand = new RelayCommand(ShowStart, CanShowStart);
            ShowOptionsCommand = new RelayCommand(ShowOptions, CanShowOptions);
            ShowGameCommand = new RelayCommand(ShowGame, CanShowGame);
            ExitCommand = new RelayCommand(Exit, CanExit);

            DataContext = this;

            if (SnakeViewModel == null)
                SnakeViewModel = new SnakeViewModel();

            InitializeComponent();
        }

        private void Initialize()
        {
            _frameCounter = new FrameCounter(RenderNotification); // TODO enable + disable

            CompositionTarget.Rendering += CompositionTarget_Rendering;
        }

        private void Dispose()
        {
            CompositionTarget.Rendering -= CompositionTarget_Rendering;

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

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            // hm still useful?
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
