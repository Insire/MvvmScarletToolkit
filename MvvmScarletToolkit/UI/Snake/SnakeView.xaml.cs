using System;
using System.Windows;
using System.Windows.Media;

namespace MvvmScarletToolkit
{
    public partial class SnakeView
    {

        public IRefresh Manager { get; }

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

        public SnakeView()
        {
            Manager = new SnakeManager();
            this.DataContext = this;


            InitializeComponent();

            CompositionTarget.Rendering += CompositionTarget_Rendering;
        }

        private void CompositionTarget_Rendering(object sender, EventArgs e)
        {
            Manager.Refresh();
        }
    }
}
