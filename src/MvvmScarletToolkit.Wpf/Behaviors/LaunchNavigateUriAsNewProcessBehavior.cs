using System;
using System.Diagnostics;
using System.Windows.Documents;
using Microsoft.Toolkit.Mvvm.Input;
using Microsoft.Xaml.Behaviors;

namespace MvvmScarletToolkit.Wpf
{
    /// <summary>
    /// Behavior that enables launching an <see cref="Uri"/> as new <see cref="Process"/> via cmd
    /// </summary>
    /// <remarks>
    /// required namespaces:
    /// <list type="bullet">
    /// <item>
    /// <description>xmlns:i="http://schemas.microsoft.com/xaml/behaviors"</description>
    /// </item>
    /// <item>
    /// <description>xmlns:mvvm="http://SoftThorn.MvvmScarletToolkit.com/winfx/xaml/shared"</description>
    /// </item>
    /// </list>
    /// </remarks>
    public sealed class LaunchNavigateUriAsNewProcessBehavior : Behavior<Hyperlink>
    {
        private RelayCommand? _command;

        protected override void OnAttached()
        {
            AssociatedObject.SetCurrentValue(Hyperlink.CommandProperty, Create());

            var propertyDescriptor = System.ComponentModel.DependencyPropertyDescriptor.FromProperty(Hyperlink.NavigateUriProperty, typeof(Hyperlink));
            propertyDescriptor?.AddValueChanged(AssociatedObject, OnNavigateUriChanged);
        }

        protected override void OnDetaching()
        {
            AssociatedObject.SetCurrentValue(Hyperlink.CommandProperty, null);
            _command = null;

            var propertyDescriptor = System.ComponentModel.DependencyPropertyDescriptor.FromProperty(Hyperlink.NavigateUriProperty, typeof(Hyperlink));
            propertyDescriptor?.RemoveValueChanged(AssociatedObject, OnNavigateUriChanged);
        }

        private void OnNavigateUriChanged(object sender, EventArgs args)
        {
            _command?.NotifyCanExecuteChanged();
        }

        private RelayCommand Create()
        {
            return _command = new RelayCommand(ExecuteImpl, CanExecuteImpl);
        }

        private void ExecuteImpl()
        {
            if (AssociatedObject.NavigateUri is Uri uri) // null check
            {
                var info = new ProcessStartInfo("cmd", $"/c start {uri}")
                {
                    CreateNoWindow = true,
                };

                using (Process.Start(info))
                { }
            }
        }

        private bool CanExecuteImpl()
        {
            var value = AssociatedObject.NavigateUri;

            return value is Uri;
        }
    }
}
