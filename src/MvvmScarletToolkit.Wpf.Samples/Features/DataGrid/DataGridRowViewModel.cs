using CommunityToolkit.Mvvm.ComponentModel;
using MvvmScarletToolkit.Observables;
using System;

namespace MvvmScarletToolkit.Wpf.Samples.Features.DataGrid
{
    public sealed partial class DataGridRowViewModel : ViewModelBase
    {
        private int _id;
        public int Id
        {
            get { return _id; }
            set
            {
                if (SetProperty(ref _id, value))
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

        private string? _color;
        public string? Color
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
            private set
            {
                SetProperty(ref _updatedOn, value);
            }
        }

        [ObservableProperty]
        private bool? _isSelected;

        public int Page { get; }

        public DataGridRowViewModel(IScarletCommandBuilder commandBuilder, int page)
            : base(commandBuilder)
        {
            Page = page;
            _name = string.Empty;
        }
    }
}
