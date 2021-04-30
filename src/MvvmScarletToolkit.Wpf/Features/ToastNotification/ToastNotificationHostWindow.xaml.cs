using System;
using System.Windows;
using System.Windows.Media.Animation;

namespace MvvmScarletToolkit.Wpf
{
    public partial class ToastNotificationHostWindow : Window
    {
        public ToastNotificationHostWindow()
        {
            InitializeComponent();

            Closing += ToastNotificationHost_Closing;
        }

        private void ToastNotificationHost_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Closing -= ToastNotificationHost_Closing;
            e.Cancel = true;
            var anim = new DoubleAnimation(0, TimeSpan.FromSeconds(0.25));
            anim.Completed += (s, _) => Close();

            BeginAnimation(OpacityProperty, anim);
        }
    }
}
