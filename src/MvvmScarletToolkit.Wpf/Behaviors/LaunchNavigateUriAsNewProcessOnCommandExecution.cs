using System;
using System.Diagnostics;
using System.Windows.Documents;
using Microsoft.Toolkit.Mvvm.Input;
using Microsoft.Xaml.Behaviors;

namespace MvvmScarletToolkit.Wpf
{
    public sealed class LaunchNavigateUriAsNewProcessOnCommandExecution : Behavior<Hyperlink>
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
