using MvvmScarletToolkit;
using MvvmScarletToolkit.Observables;
using System;

namespace DemoApp
{
    public class DataGridRowViewModel : ViewModelBase
    {
        private string _name;
        public string Name
        {
            get { return _name; }
            set { SetValue(ref _name, value, OnChanged: () => UpdatedOn = DateTime.Now); }
        }

        private string _color;
        public string Color
        {
            get { return _color; }
            set { SetValue(ref _color, value, OnChanged: () => UpdatedOn = DateTime.Now); }
        }

        private DateTime _createdOn;
        public DateTime CreatedOn
        {
            get { return _createdOn; }
            set { SetValue(ref _createdOn, value, OnChanged: () => UpdatedOn = DateTime.Now); }
        }

        private DateTime _updatedOn;
        public DateTime UpdatedOn
        {
            get { return _createdOn; }
            private set { SetValue(ref _updatedOn, value); }
        }

        private bool _isSelected;
        public bool IsSelected
        {
            get { return _isSelected; }
            set { SetValue(ref _isSelected, value); }
        }

        public DataGridRowViewModel(ICommandBuilder commandBuilder)
            : base(commandBuilder)
        {
        }
    }
}
