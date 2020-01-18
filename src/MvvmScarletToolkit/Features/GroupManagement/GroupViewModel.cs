using MvvmScarletToolkit.Observables;
using System.Reflection;
using System.Windows.Data;

namespace MvvmScarletToolkit
{
    public sealed class GroupViewModel : ViewModelBase<PropertyInfo>
    {
        private string _name;
        public string Name
        {
            get { return _name; }
            private set { SetValue(ref _name, value); }
        }

        public PropertyGroupDescription GroupDescription { get; }

        public GroupViewModel(ICommandBuilder commandBuilder, PropertyInfo propertyInfo)
            : base(commandBuilder, propertyInfo)
        {
            _name = Model?.Name ?? string.Empty;
            GroupDescription = new PropertyGroupDescription(propertyInfo.Name);
        }
    }
}
