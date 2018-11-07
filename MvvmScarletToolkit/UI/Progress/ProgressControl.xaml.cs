using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace MvvmScarletToolkit.UI.Progress
{
    public partial class ProgressControl : UserControl
    {
        public ProgressControl()
        {
            InitializeComponent();

            Angle = (Percentage * 360) / 100;
        }

        public int Radius
        {
            get { return (int)GetValue(RadiusProperty); }
            set { SetValue(RadiusProperty, value); }
        }

        public Brush SegmentColor
        {
            get { return (Brush)GetValue(SegmentColorProperty); }
            set { SetValue(SegmentColorProperty, value); }
        }

        public int StrokeThickness
        {
            get { return (int)GetValue(StrokeThicknessProperty); }
            set { SetValue(StrokeThicknessProperty, value); }
        }

        public double Percentage
        {
            get { return (double)GetValue(PercentageProperty); }
            set { SetValue(PercentageProperty, value); }
        }

        public double Angle
        {
            get { return (double)GetValue(AngleProperty); }
            set { SetValue(AngleProperty, value); }
        }

        public static readonly DependencyProperty PercentageProperty = DependencyProperty.Register(
            nameof(Percentage),
            typeof(double),
            typeof(ProgressControl),
            new PropertyMetadata(0d, new PropertyChangedCallback(OnPercentageChanged)));

        public static readonly DependencyProperty StrokeThicknessProperty = DependencyProperty.Register(
            nameof(StrokeThickness),
            typeof(int),
            typeof(ProgressControl),
            new PropertyMetadata(5, new PropertyChangedCallback(OnThicknessChanged)));

        public static readonly DependencyProperty SegmentColorProperty = DependencyProperty.Register(
            nameof(SegmentColor),
            typeof(Brush),
            typeof(ProgressControl),
            new PropertyMetadata(new SolidColorBrush(Colors.Red), new PropertyChangedCallback(OnColorChanged)));

        public static readonly DependencyProperty RadiusProperty = DependencyProperty.Register(
            nameof(Radius),
            typeof(int),
            typeof(ProgressControl),
            new PropertyMetadata(25, new PropertyChangedCallback(OnPropertyChanged)));

        public static readonly DependencyProperty AngleProperty = DependencyProperty.Register(
            nameof(Angle),
            typeof(double),
            typeof(ProgressControl),
            new PropertyMetadata(120d, new PropertyChangedCallback(OnPropertyChanged)));

        private static void OnColorChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            if (!(sender is ProgressControl circle))
                return;

            circle.SetColor((SolidColorBrush)args.NewValue);
        }

        private static void OnThicknessChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            if (!(sender is ProgressControl circle))
                return;

            circle.SetTick((int)args.NewValue);
        }

        private static void OnPercentageChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            if (!(sender is ProgressControl circle))
                return;

            if (circle.Percentage > 100)
                circle.Percentage = 100;

            circle.Angle = (circle.Percentage * 360) / 100;
        }

        private static void OnPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            if (!(sender is ProgressControl circle))
                return;

            circle.RenderArc();
        }

        public void SetTick(int n)
        {
            PathRoot.StrokeThickness = n;
        }

        public void SetColor(SolidColorBrush n)
        {
            PathRoot.Stroke = n;
        }

        private void RenderArc()
        {
            var startPoint = new Point(Radius, 0);
            var endPoint = ComputeCartesianCoordinate(Angle, Radius);

            endPoint.X += Radius;
            endPoint.Y += Radius;

            PathRoot.Width = (Radius * 2) + StrokeThickness;
            PathRoot.Height = (Radius * 2) + StrokeThickness;
            PathRoot.Margin = new Thickness(StrokeThickness, StrokeThickness, 0, 0);

            var outerArcSize = new System.Windows.Size(Radius, Radius);

            PathFigure.StartPoint = startPoint;

            if (startPoint.X == Math.Round(endPoint.X) && startPoint.Y == Math.Round(endPoint.Y))
                endPoint.X -= 0.01;

            if (Percentage == 0)
            {
                ArcSegment.Point = endPoint;
                ArcSegment.Size = outerArcSize;
                ArcSegment.IsLargeArc = Angle > 180.0;

                PathRoot.Visibility = Visibility.Hidden;
            }
            else
            {
                ArcSegment.Size = outerArcSize;
                ArcSegment.IsLargeArc = Angle > 180.0;
                var animation = GetAnimation(ArcSegment.Point, endPoint);
                animation.Begin(this);

                PathRoot.Visibility = Visibility.Visible;
            }
        }

        private Storyboard GetAnimation(Point from, Point to)
        {
            var pointAnimation = new PointAnimation()
            {
                Duration = TimeSpan.FromMilliseconds(250),
                From = from,
                To = to,
            };

            Storyboard.SetTargetName(pointAnimation, nameof(ArcSegment));
            Storyboard.SetTargetProperty(pointAnimation, new PropertyPath(ArcSegment.PointProperty));

            var ellipseStoryboard = new Storyboard();
            ellipseStoryboard.Children.Add(pointAnimation);

            return ellipseStoryboard;
        }

        private static Point ComputeCartesianCoordinate(double angle, double radius)
        {
            // convert to radians
            var angleRad = (Math.PI / 180.0) * (angle - 90);

            var x = radius * Math.Cos(angleRad);
            var y = radius * Math.Sin(angleRad);

            return new Point(x, y);
        }
    }
}
