using MvvmScarletToolkit;
using MvvmScarletToolkit.Observables;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace DemoApp
{
    public class DataGridViewModel : BusinessViewModelListBase<DataGridRowViewModel>
    {
        public DataGridViewModel(ICommandBuilder commandBuilder)
            : base(commandBuilder)
        {
        }

        protected override async Task RefreshInternal(CancellationToken token)
        {
            for (var i = 0; i < 50; i++)
            {
                await Add(new DataGridRowViewModel(CommandBuilder)
                {
                    CreatedOn = DateTime.Now,
                    Name = Guid.NewGuid().ToString(),
                });
            }
        }
    }

    public class DataGridRowViewModel : ViewModelBase
    {
        private string _name;
        public string Name
        {
            get { return _name; }
            set { SetValue(ref _name, value, OnChanged: () => UpdatedOn = DateTime.Now); }
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
