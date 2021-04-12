using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace MvvmScarletToolkit.Wpf
{
    [TemplatePart(Name = "PART_DismissButton", Type = typeof(Button))]
    public class ToastNotification : ContentControl
    {
        static ToastNotification()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(ToastNotification), new FrameworkPropertyMetadata(typeof(ToastNotification)));
        }

        public string Title
        {
            get { return (string)GetValue(TitleProperty); }
            set { SetValue(TitleProperty, value); }
        }

        public static readonly DependencyProperty TitleProperty = DependencyProperty.Register(
            "Title",
            typeof(string),
            typeof(ToastNotification));

        public string Message
        {
            get { return (string)GetValue(MessageProperty); }
            set { SetValue(MessageProperty, value); }
        }

        public static readonly DependencyProperty MessageProperty = DependencyProperty.Register(
            "Message",
            typeof(string),
            typeof(ToastNotification));

        public ToastType ToastType
        {
            get { return (ToastType)GetValue(ToastTypeProperty); }
            set { SetValue(ToastTypeProperty, value); }
        }

        public static readonly DependencyProperty ToastTypeProperty = DependencyProperty.Register(
            "ToastType",
            typeof(ToastType),
            typeof(ToastNotification),
            new PropertyMetadata(new PropertyChangedCallback(OnToastTypeChanged)));

        public bool IsPersistent
        {
            get { return (bool)GetValue(IsPersistentProperty); }
            set { SetValue(IsPersistentProperty, value); }
        }

        public static readonly DependencyProperty IsPersistentProperty = DependencyProperty.Register(
            "IsPersistent",
            typeof(bool),
            typeof(ToastNotification));

        public double FontSizeTitle
        {
            get { return (double)GetValue(FontSizeTitleProperty); }
            set { SetValue(FontSizeTitleProperty, value); }
        }

        public static readonly DependencyProperty FontSizeTitleProperty = DependencyProperty.Register(
            "FontSizeTitle",
            typeof(double),
            typeof(ToastNotification));

        public Brush ColourSuccess
        {
            get { return (Brush)GetValue(ColourSuccessProperty); }
            set { SetValue(ColourSuccessProperty, value); }
        }

        public static readonly DependencyProperty ColourSuccessProperty = DependencyProperty.Register(
            "ColourSuccess",
            typeof(Brush),
            typeof(ToastNotification));

        public Brush ColourDanger
        {
            get { return (Brush)GetValue(ColourDangerProperty); }
            set { SetValue(ColourDangerProperty, value); }
        }

        public static readonly DependencyProperty ColourDangerProperty = DependencyProperty.Register(
            "ColourDanger",
            typeof(Brush),
            typeof(ToastNotification));

        public Brush ColourInfo
        {
            get { return (Brush)GetValue(ColourInfoProperty); }
            set { SetValue(ColourInfoProperty, value); }
        }

        public static readonly DependencyProperty ColourInfoProperty = DependencyProperty.Register(
            "ColourInfo",
            typeof(Brush),
            typeof(ToastNotification));

        public Brush ColourWarning
        {
            get { return (Brush)GetValue(ColourWarningProperty); }
            set { SetValue(ColourWarningProperty, value); }
        }

        public static readonly DependencyProperty ColourWarningProperty = DependencyProperty.Register(
            "ColourWarning",
            typeof(Brush),
            typeof(ToastNotification));

        public event EventHandler? Dismissed;

        public ToastNotification()
        {
            Loaded += ToastNotification_Loaded;
        }

        private void ToastNotification_Loaded(object sender, RoutedEventArgs e)
        {
            if (FindResource("ToastScaleInStoryboard") is Storyboard sb)
            {
                Storyboard.SetTarget(sb, this);
                sb.Begin();
            }
        }

        public override void OnApplyTemplate()
        {
            var PART_DismissButton = GetTemplateChild("PART_DismissButton") as ButtonBase;

            if (PART_DismissButton != null)
            {
                PART_DismissButton.Click += OnDismissed;
            }

            base.OnApplyTemplate();

            RefreshBackgroundColour();
        }

        protected void OnDismissed(object sender, RoutedEventArgs e)
        {
            Dismissed?.Invoke(this, EventArgs.Empty);
        }

        private static void OnToastTypeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is ToastNotification toastNotification)
            {
                toastNotification.RefreshBackgroundColour();
            }
        }

        private void RefreshBackgroundColour()
        {
            switch (ToastType)
            {
                case ToastType.Success:
                    Background = ColourSuccess;
                    break;

                case ToastType.Error:
                    Background = ColourDanger;
                    break;

                case ToastType.Information:
                    Background = ColourInfo;
                    break;

                case ToastType.Warning:
                    Background = ColourWarning;
                    break;
            }
        }
    }
}
