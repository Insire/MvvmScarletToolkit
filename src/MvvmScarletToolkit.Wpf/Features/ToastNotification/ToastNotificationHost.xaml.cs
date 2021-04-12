using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Media.Animation;

namespace MvvmScarletToolkit.Wpf
{
    public partial class ToastNotificationHost : Window
    {
        public ObservableCollection<ToastNotification> Toasts
        {
            get { return (ObservableCollection<ToastNotification>)GetValue(ToastsProperty); }
            set { SetValue(ToastsProperty, value); }
        }

        public static readonly DependencyProperty ToastsProperty = DependencyProperty.Register(
            nameof(Toasts),
            typeof(ObservableCollection<ToastNotification>),
            typeof(ToastNotificationHost),
            new PropertyMetadata(new ObservableCollection<ToastNotification>()));

        public Rect? DisplayOrigin
        {
            get { return (Rect?)GetValue(DisplayOriginProperty); }
            set { SetValue(DisplayOriginProperty, value); }
        }

        public static readonly DependencyProperty DisplayOriginProperty = DependencyProperty.Register(
            nameof(DisplayOrigin),
            typeof(Rect?),
            typeof(ToastNotificationHost),
            new PropertyMetadata(null, new PropertyChangedCallback(DisplayOriginChanged)));

        public ToastNotificationHost()
        {
            InitializeComponent();

            Closing += ToastNotificationHost_Closing;
            Loaded += ToastNotification_Loaded;
        }

        private static void DisplayOriginChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var host = (ToastNotificationHost)d;

            host.Reposition();
        }

        public void Reposition()
        {
            const int offset = 12;

            var area = DisplayOrigin != null ? (Rect)DisplayOrigin : SystemParameters.WorkArea;

            //Display the toast at the top right of the area.
            Left = area.Right - Width - offset;
            Top = area.Top + offset;
        }

        private void ToastNotificationHost_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Closing -= ToastNotificationHost_Closing;
            e.Cancel = true;
            var anim = new DoubleAnimation(0, TimeSpan.FromSeconds(0.25));
            anim.Completed += (s, _) => Close();
            BeginAnimation(OpacityProperty, anim);
        }

        private void ToastNotification_Loaded(object sender, RoutedEventArgs e)
        {
            Reposition();
        }
    }
}
