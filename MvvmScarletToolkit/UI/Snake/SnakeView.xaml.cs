using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;

namespace MvvmScarletToolkit
{
    public partial class SnakeView
    {
        private readonly Dispatcher _dispatcher;

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

        public bool ShowFps
        {
            get { return (bool)GetValue(ShowFpsProperty); }
            set { SetValue(ShowFpsProperty, value); }
        }

        public static readonly DependencyProperty ShowFpsProperty = DependencyProperty.Register(
            "ShowFps",
            typeof(bool),
            typeof(SnakeView),
            new PropertyMetadata(false));

        public SnakeView()
        {
            _dispatcher = Application.Current.Dispatcher;
            DataContext = this;

            InitializeComponent();
        }

        private void Initialize()
        {
            Manager = new SnakeManager(SnakeOptions.Normal(), _dispatcher, new InternalLogger());
            _frameCounter = new FrameCounter(RenderNotification);

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
            Initialize();
        }

        private void SnakeView_Unloaded(object sender, RoutedEventArgs e)
        {
            Dispose();
        }
    }
}
