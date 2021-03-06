using MvvmScarletToolkit.Observables;
using System;

namespace MvvmScarletToolkit.Wpf.Samples
{
    public sealed class DataGridRowViewModel : ViewModelBase
    {
        private int id;
        public int Id
        {
            get { return id; }
            set
            {
                if (SetProperty(ref id, value))
                {
                    UpdatedOn = DateTime.Now;
                }
            }
        }

        private string _name;
        public string Name
        {
            get { return _name; }
            set
            {
                if (SetProperty(ref _name, value))
                {
                    UpdatedOn = DateTime.Now;
                }
            }
        }

        private string _color;
        public string Color
        {
            get { return _color; }
            set
            {
                if (SetProperty(ref _color, value))
                {
                    UpdatedOn = DateTime.Now;
                }
            }
        }

        private DateTime _createdOn;
        public DateTime CreatedOn
        {
            get { return _createdOn; }
            set
            {
                if (SetProperty(ref _createdOn, value))
                {
                    UpdatedOn = DateTime.Now;
                }
            }
        }

        private DateTime _updatedOn;
        public DateTime UpdatedOn
        {
            get { return _updatedOn; }
            private set { SetProperty(ref _updatedOn, value); }
        }

        private bool _isSelected;
        public bool IsSelected
        {
            get { return _isSelected; }
            set { SetProperty(ref _isSelected, value); }
        }

        public int Page { get; }

        public DataGridRowViewModel(IScarletCommandBuilder commandBuilder, int page)
            : base(commandBuilder)
        {
            Page = page;
        }
    }
}
