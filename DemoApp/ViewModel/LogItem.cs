using System;
using MvvmScarletToolkit;

namespace DemoApp
{
    public class LogItem : ObservableObject
    {
        private string _message;
        public string Message
        {
            get { return _message; }
            set { SetValue(ref _message, value); }
        }

        private DateTime _createdOn;
        public DateTime CreatedOn
        {
            get { return _createdOn; }
            set { SetValue(ref _createdOn, value); }
        }

        private bool _isSelected;
        public bool IsSelected
        {
            get { return _isSelected; }
            set { SetValue(ref _isSelected, value); }
        }

        public LogItem()
        {
            CreatedOn = DateTime.UtcNow;
            Message = "unknown";
        }

        public LogItem(string message):this()
        {
            Message = message;
        }
    }
}
