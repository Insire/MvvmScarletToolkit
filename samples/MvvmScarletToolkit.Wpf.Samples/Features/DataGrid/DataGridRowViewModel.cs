using CommunityToolkit.Mvvm.ComponentModel;
using MvvmScarletToolkit.Observables;

namespace MvvmScarletToolkit.Wpf.Samples.Features.DataGrid
{
    public sealed partial class DataGridRowViewModel : ViewModelBase
    {
        public int Id
        {
            get;
            set
            {
                if (SetProperty(ref field, value))
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

        public string? Color
        {
            get;
            set
            {
                if (SetProperty(ref field, value))
                {
                    UpdatedOn = DateTime.Now;
                }
            }
        }

        public DateTime CreatedOn
        {
            get;
            set
            {
                if (SetProperty(ref field, value))
                {
                    UpdatedOn = DateTime.Now;
                }
            }
        }

        [ObservableProperty]
        public partial DateTime UpdatedOn { get; private set; }

        [ObservableProperty]
        public partial bool? IsSelected { get; set; }
        public int Page { get; }

        public DataGridRowViewModel(IScarletCommandBuilder commandBuilder, int page)
            : base(commandBuilder)
        {
            Page = page;
            _name = string.Empty;
        }
    }
}
