using System;
using System.Windows;

namespace MvvmScarletToolkit
{
    public class Scene : ObservableObject
    {
        private UIElement _control;
        public UIElement Control
        {
            get { return _control; }
            set { SetValue(ref _control, value); }
        }

        private string _name;
        public string Name
        {
            get { return _name; }
            set { SetValue(ref _name, value); }
        }
    }
}
